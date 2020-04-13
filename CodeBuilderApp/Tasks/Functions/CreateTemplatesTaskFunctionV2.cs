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
    public sealed partial class CreateTemplatesTaskFunctionV2 : ITaskFunction
    {
        public string Name => "Create templates";

        private Task<(TaskReturnKind, TagElement?)> CreateTags()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Input tag:");
            string tag = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(tag))
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Tag cant be empty try again!");
                return Task.FromResult((TaskReturnKind.Continue, (TagElement?)null));
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Replace text:");
            string text = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Replace text cant be empty try again!");
                return Task.FromResult((TaskReturnKind.Continue, (TagElement?)null));
            }

            Console.WriteLine($"Tag {tag}");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Replace text {text}");
            Console.WriteLine(Environment.NewLine);

            var element = new TagElement($"{tag}", text);
            Console.WriteLine("Continue creating tags y/n?");
            string response = Console.ReadLine();
            if (response == "y")
                return Task.FromResult((TaskReturnKind.Continue, (TagElement?)element));

            return Task.FromResult((TaskReturnKind.Exit, (TagElement?)element));
        }

        private async Task<(TaskReturnKind, IEnumerable<TagElement>?)> LoadTags()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select tag file to load:");
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
                IEnumerable<TagElement> tags = JsonConvert.DeserializeObject<TagElement[]>(json);
                return (TaskReturnKind.Exit, tags);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong!!");
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(e.Message);

                return (TaskReturnKind.Continue, default);
            }
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
            workspace.SkipUnrecognizedProjects = true;

            Solution solution = await workspace.OpenSolutionAsync(solutionPath);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select project:");
            Project? project = await TaskExecutable.RunTask(CommonTaskFunctions.GetProjectTask, solution);
            if (project == null)
                return;

            Console.WriteLine(Environment.NewLine);
            TagOption? tagOption = await TaskExecutable.RunTask(this.SelectTagOption);
            if (!tagOption.HasValue)
                return;

            IEnumerable<TagElement> tags = await this.GetTagElements(tagOption.Value);
            IEnumerable<Document> documents = await CommonTaskFunctions.SelectDocuments(project);
            TagProjectGroup projectGroup = await CommonTaskFunctions.ImplementsTags(documents, tags);
            await TaskExecutable.RunTask(CommonTaskFunctions.SaveDocumentsTask, projectGroup);

            workspace.CloseSolution();
        }

        private async Task<IEnumerable<TagElement>> GetTagElements(TagOption option)
        {
            switch (option)
            {
                case TagOption.CreateTags:
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine("Create tags:");
                    var tags = new List<TagElement>();
                    await foreach (var tagElement in TaskExecutable.RunTaskAsyncEnumerable(this.CreateTags))
                    {
                        if (tagElement != null)
                            tags.Add(tagElement);
                    }

                    return tags;

                case TagOption.LoadTags:
                    IEnumerable<TagElement>? loadedTags = await TaskExecutable.RunTask(this.LoadTags);
                    if (loadedTags == null)
                        return Enumerable.Empty<TagElement>();

                    return loadedTags;
            }

            return Enumerable.Empty<TagElement>();
        }

        private Task<(TaskReturnKind, TagOption?)> SelectTagOption()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select tag option:");
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine($"{(int)TagOption.CreateTags}: Create tags");
            Console.WriteLine($"{(int)TagOption.LoadTags}: Load tags");
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

        private enum TagOption
        {
            CreateTags = 0,
            LoadTags = 1
        }
    }
}