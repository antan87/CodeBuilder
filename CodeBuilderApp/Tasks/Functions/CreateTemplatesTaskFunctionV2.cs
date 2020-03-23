using CodeBuilderApp.Extensions;
using CodeBuilderApp.Tagging;
using CodeBuilderApp.Tasks.Interfaces;
using CodeBuilderWorkspace.Workspace.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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

            using var workspace = await new MSBuildWorkspaceFactory().GetWorkspace();
            Solution solution = await workspace.OpenSolutionAsync(solutionPath);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select project:");
            Project? project = await TaskExecutable.RunTask(CommonTaskFunctions.GetProjectTask, solution);
            if (project == null)
                return;

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Create tags:");
            var tags = new List<TagElement>();
            await foreach (var tagElement in TaskExecutable.RunTask(this.CreateTags))
            {
                if (tagElement != null)
                    tags.Add(tagElement);
            }

            var documents = new List<Document>();
            await foreach (var document in TaskExecutable.RunTask(this.SelectDocuments, project))
            {
                if (document != null)
                    documents.Add(document);
            }

            ProjectGroup projectGroup = await ImplementsTags(documents, tags);
            await TaskExecutable.RunTask(CommonTaskFunctions.SaveDocumentsTask, projectGroup);

            workspace.CloseSolution();
        }

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

        private async Task<ProjectGroup> ImplementsTags(List<Document> documents, List<TagElement> tags)
        {
            IEnumerable<Task<DocumentGroup>> tasks = documents.Select(document => TagDocument(document, tags));
            DocumentGroup[] documentGroups = await Task.WhenAll(tasks);

            return new ProjectGroup(documentGroups, tags);
        }

        private async Task<DocumentGroup> TagDocument(Document document, List<TagElement> tags)
        {
            string folder = string.Join("/", document.Folders);
            string name = document.Name;
            SourceText sourceText = await document.GetTextAsync();
            string text = sourceText.ToString();
            var documentGroup = new DocumentGroup(folder, name, text);

            foreach (TagElement tag in tags)
            {
                string newFolderText = documentGroup.Folder.ReplaceTextWithTag(tag.ReplaceText, tag.Tag);
                string newNameText = documentGroup.Name.ReplaceTextWithTag(tag.ReplaceText, tag.Tag);
                string newText = documentGroup.Text.ReplaceTextWithTag(tag.ReplaceText, tag.Tag);

                documentGroup = new DocumentGroup(newFolderText, newNameText, newText);
            }

            return documentGroup;
        }

        private Task<(TaskReturnKind, Document?)> SelectDocuments(Project project)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select document:");
            Dictionary<string, Document> documents = new Dictionary<string, Document>();
            int index = 1;
            foreach (Document document in project.Documents)
            {
                Console.WriteLine($"{index}: {(document.Folders.Any() ? string.Join(@"\", document.Folders) + @"\" : string.Empty)}{document.Name}");

                documents[index.ToString()] = document;
                index++;
            }

            string documentIndex = Console.ReadLine();
            if (!documents.ContainsKey(documentIndex))
            {
                Console.WriteLine("Wrong input try again!");
                return Task.FromResult((TaskReturnKind.Continue, (Document?)null));
            }
            Document selecteDocument = documents[documentIndex];
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Continue selecting documents y/n?");
            string response = Console.ReadLine();
            if (response == "y")
                return Task.FromResult((TaskReturnKind.Continue, (Document?)selecteDocument));

            return Task.FromResult((TaskReturnKind.Exit, (Document?)selecteDocument));
        }
    }
}