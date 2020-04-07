using CodeBuilderApp.Common;
using CodeBuilderApp.Extensions;
using CodeBuilderApp.Tagging;
using CodeBuilderApp.Tasks.Interfaces;
using CodeBuilderWorkspace.Workspace.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        private async Task<IEnumerable<CreateDocumentGroup>> GetDocumentsToCreate(TagOption option, IEnumerable<TagElement> tags)
        {
            switch (option)
            {
                case TagOption.ReplaceTags:
                    return this.ReplaceTags(tags);

                case TagOption.LoadReplacedTags:
                    IEnumerable<CreateDocumentGroup>? loadedTags = await TaskExecutable.RunTask(this.LoadReplacedTags);
                    if (loadedTags == null)
                        return Enumerable.Empty<CreateDocumentGroup>();

                    return loadedTags;
            }

            return Enumerable.Empty<CreateDocumentGroup>();
        }

        private TagDocumentGroup ReplaceDocumentTags(TagDocumentGroup documentGroup, IEnumerable<ReplacedTagElement> tags)
        {
            documentGroup = this.ReplaceFolderTags(documentGroup, tags);
            documentGroup = this.ReplaceNameTags(documentGroup, tags);
            documentGroup = this.ReplaceTextTags(documentGroup, tags);

            return documentGroup;
        }

        private TagDocumentGroup ReplaceFolderTags(TagDocumentGroup documentGroup, IEnumerable<ReplacedTagElement> tags)
        {
            foreach (ReplacedTagElement tag in tags)
            {
                string replacedFolderText = documentGroup.Folder.ReplaceTagWithText(tag.NewContent, tag.Tag);
                documentGroup = new TagDocumentGroup(replacedFolderText, documentGroup.Name, documentGroup.Text);
            }

            return documentGroup;
        }

        private TagDocumentGroup ReplaceNameTags(TagDocumentGroup documentGroup, IEnumerable<ReplacedTagElement> tags)
        {
            foreach (ReplacedTagElement tag in tags)
            {
                string replacedNameText = documentGroup.Name.ReplaceTagWithText(tag.NewContent, tag.Tag);
                documentGroup = new TagDocumentGroup(documentGroup.Folder, replacedNameText, documentGroup.Text);
            }

            return documentGroup;
        }

        private TagDocumentGroup ReplaceTextTags(TagDocumentGroup documentGroup, IEnumerable<ReplacedTagElement> tags)
        {
            foreach (ReplacedTagElement tag in tags)
            {
                string replaceText = documentGroup.Text.ReplaceTagWithText(tag.NewContent, tag.Tag);
                documentGroup = new TagDocumentGroup(documentGroup.Folder, documentGroup.Name, replaceText);
            }

            return documentGroup;
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

            await foreach (var result in TaskExecutable.RunTaskAsyncEnumerable(this.SelectProjectTask, solution))
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

                IEnumerable<CreateDocumentGroup> createDocuments = await this.GetDocumentsToCreate(tagOption.Value, projectGroup.Tags);
                List<Document> documents = new List<Document>();
                foreach (CreateDocumentGroup createDocument in createDocuments)
                {
                    foreach (TagDocumentGroup documentGroup in projectGroup.Documents)
                    {
                        TagDocumentGroup newDocumentGroup = this.ReplaceDocumentTags(documentGroup, createDocument.ReplacedTags);
                        Document? newDocument = this.AppendDocumentToProject(project, newDocumentGroup);
                        if (newDocument != null)
                        {
                            project = newDocument.Project;
                            documents.Add(newDocument);
                        }
                    }
                }

                var createProjectGroup = new CreateProjectGroup(project, documents);

                return (TaskReturnKind.Continue, createProjectGroup);
            }

            return (TaskReturnKind.Exit, null);
        }

        private IEnumerable<CreateDocumentGroup> ReplaceTags(IEnumerable<TagElement> tagElements)
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

            return new List<CreateDocumentGroup> { new CreateDocumentGroup(list) };
        }

        private Document? AppendDocumentToProject(Project project, TagDocumentGroup documentGroup)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentGroup.Text);
            if (syntaxTree.TryGetRoot(out SyntaxNode node))
                return project.AddDocument(documentGroup.Name, node, documentGroup.Folder.Split(@"\"));

            return null;
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

        private async Task<(TaskReturnKind, IEnumerable<CreateDocumentGroup>?)> LoadReplacedTags()
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
                IEnumerable<CreateDocumentGroup> tags = JsonConvert.DeserializeObject<CreateDocumentGroup[]>(json);
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