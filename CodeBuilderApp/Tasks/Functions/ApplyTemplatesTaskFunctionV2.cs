using CodeBuilderApp.Common;
using CodeBuilderApp.Tagging;
using CodeBuilderApp.Tasks.Interfaces;
using CodeBuilderWorkspace.Workspace.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeBuilderApp.Tasks.Functions
{
    public sealed class ApplyTemplatesTaskFunctionV2 : ITaskFunction
    {
        public string Name => "Apply templates";

        private async Task<IEnumerable<DocumentGroup>> GetDocumentsToCreate(TagOption option, IEnumerable<TagElement> tags)
        {
            switch (option)
            {
                case TagOption.ReplaceTags:
                    return this.ReplaceTags(tags);

                case TagOption.LoadReplacedTags:
                    IEnumerable<DocumentGroup>? loadedTags = await TaskExecutable.RunTask(this.LoadReplacedTags);
                    if (loadedTags == null)
                        return Enumerable.Empty<DocumentGroup>();

                    return loadedTags;
            }

            return Enumerable.Empty<DocumentGroup>();
        }

        public async Task RunTask()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(this.Name);
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Input solution path.");
            Console.WriteLine(Environment.NewLine);
            string solutionPath = Console.ReadLine();
            if (!File.Exists(solutionPath))
            {
                Console.WriteLine("File does not exists!");
                return;
            }

            using MSBuildWorkspace workspace = await new MSBuildWorkspaceFactory().GetWorkspace();
            Solution solution = await workspace.OpenSolutionAsync(solutionPath);

            await foreach (CreateProjectGroup? result in TaskExecutable.RunTaskAsyncEnumerable(this.SelectProjectTask, solution))
            {
                if (result == null || result.Project == null)
                    continue;

                workspace.TryApplyChanges(result.Project.Solution);
            }

            workspace.CloseSolution();
        }

        private async Task<(TaskReturnKind, CreateProjectGroup?)> SelectProjectTask(Solution solution)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select project:");
            Project? project = await TaskExecutable.RunTask(CommonTaskFunctions.GetProjectTask, solution);
            if (project == null)
                return (TaskReturnKind.Continue, null);

            await foreach (TagProjectGroup? projectGroup in TaskExecutable.RunTaskAsyncEnumerable(this.SelectTemplateFileTask, project))
            {
                if (projectGroup == null)
                    continue;

                Console.WriteLine(Environment.NewLine);
                TagOption? tagOption = await TaskExecutable.RunTask(this.SelectTagOption);
                if (!tagOption.HasValue)
                    return (TaskReturnKind.Exit, null);

                IEnumerable<DocumentGroup> createDocuments = await this.GetDocumentsToCreate(tagOption.Value, projectGroup.Tags);
                List<Document> documents = new List<Document>();
                foreach (DocumentGroup createDocument in createDocuments)
                {
                    foreach (TagDocumentGroup documentGroup in projectGroup.Documents)
                    {
                        TagDocumentGroup newDocumentGroup = CommonTaskFunctions.ReplaceDocumentTags(documentGroup, createDocument.ReplacedTags);
                        Document? newDocument = CommonTaskFunctions.AppendDocumentToProject(project, newDocumentGroup);
                        if (newDocument != null)
                        {
                            project = newDocument.Project;
                            documents.Add(newDocument);
                        }
                    }
                }

                return (TaskReturnKind.Continue, new CreateProjectGroup(project, documents));
            }

            return (TaskReturnKind.Exit, null);
        }

        private IEnumerable<DocumentGroup> ReplaceTags(IEnumerable<TagElement> tagElements)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Replace tags:");

            var list = new List<ReplacedTagElement>();
            foreach (TagElement element in tagElements)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"Tag: {element.Tag} ");
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"Input text to replace tag with.");
                string newContent = Console.ReadLine();
                list.Add(new ReplacedTagElement(element.Tag, newContent));
            }

            return new List<DocumentGroup> { new DocumentGroup(list) };
        }

        private async Task<(TaskReturnKind, TagProjectGroup?)> SelectTemplateFileTask(Project project)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select template file:");
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Input template file.");
            Console.WriteLine(Environment.NewLine);
            string templateFilePath = Console.ReadLine();
            if (!File.Exists(templateFilePath) || Path.GetExtension(templateFilePath) != FileExtensions.FileTemplateExtension)
            {
                Console.WriteLine("Wrong file input.");
                return (TaskReturnKind.Continue, default);
            }

            string json = await File.ReadAllTextAsync(templateFilePath);

            TagProjectGroup projectGroup = JsonConvert.DeserializeObject<TagProjectGroup>(json);

            return (TaskReturnKind.Exit, projectGroup);
        }

        private Task<(TaskReturnKind, TagOption?)> SelectTagOption()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select tag option:");
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine($"{(int)TagOption.ReplaceTags}: Replace tags");
            Console.WriteLine($"{(int)TagOption.LoadReplacedTags}: Load replaced tags");
            Console.WriteLine(Environment.NewLine);

            string selectedOption = Console.ReadLine();
            TagOption? enumOption = Enum.Parse<TagOption>(selectedOption);
            if (!enumOption.HasValue)
            {
                Console.WriteLine("Wrong input!");
                Console.WriteLine(Environment.NewLine);
                return Task.FromResult((TaskReturnKind.Continue, (TagOption?)default));
            }

            return Task.FromResult((TaskReturnKind.Exit, enumOption));
        }

        private async Task<(TaskReturnKind, IEnumerable<DocumentGroup>?)> LoadReplacedTags()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select replaced tag file to load:");
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine(Environment.NewLine);
            string tagFilePath = Console.ReadLine();
            if (!File.Exists(tagFilePath) || Path.GetExtension(tagFilePath) != FileExtensions.TagFileTemplateExtension)
            {
                Console.WriteLine("Wrong file input.");
                return (TaskReturnKind.Continue, default);
            }

            string json = await File.ReadAllTextAsync(tagFilePath);

            try
            {
                IEnumerable<DocumentGroup> tags = JsonConvert.DeserializeObject<DocumentGroup[]>(json);
                return (TaskReturnKind.Exit, tags);
            }
            catch (Exception e)
            {
                Console.WriteLine("Somethin went wrong!!");
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(e.Message);

                return (TaskReturnKind.Continue, default);
            }
        }

        private enum TagOption
        {
            ReplaceTags = 0,
            LoadReplacedTags = 1
        }
    }
}