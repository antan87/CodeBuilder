using CodeBuilderApp.Tagging;
using CodeBuilderApp.Tasks.Interfaces;
using CodeBuilderWorkspace.Workspace.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CodeBuilderApp.Tasks.Functions
{
    internal class CreateTemplatesTaskFunction : ITaskFunction
    {
        public string Name => "Create class templates";

        public async Task RunTask()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Create class templates:");
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

            Console.WriteLine("Select project:");
            Project? project = await TaskExecutable.RunTask(GetProjectTask, solution);
            if (project == null)
                return;

            List<DocumentGroup> documentGroups = new List<DocumentGroup>();
            await foreach (DocumentGroup? documentGroup in TaskExecutable.RunTask(GetDocumentTask, project))
                if (documentGroup != null)
                    documentGroups.Add(documentGroup);
            var projectGroup = new ProjectGroup(documentGroups);
            await TaskExecutable.RunTask(SaveDocumentsTask, projectGroup);

            workspace.CloseSolution();
        }

        private async Task<(TaskReturnKind, DocumentGroup?)> GetDocumentTask(Project project)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select document:");
            Dictionary<string, Document> documents = new Dictionary<string, Document>();
            int index = 1;
            foreach (Document document in project.Documents)
            {
                Console.WriteLine($"{index}: {document.Name}");
                documents[index.ToString()] = document;
                index++;
            }

            string documentIndex = Console.ReadLine();
            if (!documents.ContainsKey(documentIndex))
            {
                Console.WriteLine("Wrong input try again!");
                return (TaskReturnKind.Continue, (DocumentGroup?)null);
            }
            Document selecteDocument = documents[documentIndex];
            SourceText text = await selecteDocument.GetTextAsync();
            string folder = string.Join("/", selecteDocument.Folders);
            DocumentGroup documentGroup = new DocumentGroup(folder, selecteDocument.Name, text.ToString());
            if (!string.IsNullOrWhiteSpace(documentGroup.Folder))
                documentGroup = await TaskExecutable.RunTask<DocumentGroup>(TagFolder, documentGroup);

            documentGroup = await TaskExecutable.RunTask<DocumentGroup>(TagName, documentGroup);
            documentGroup = await TaskExecutable.RunTask<DocumentGroup>(TagDocument, documentGroup);

            return (TaskReturnKind.Exit, documentGroup);
        }

        private Task<TaskReturnKind> SaveDocumentsTask(ProjectGroup projectGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select folder:");
            string folderPath = Console.ReadLine();
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Folder does not exists!");
                return Task.FromResult(TaskReturnKind.Continue);
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select file name:");
            string fileName = Console.ReadLine();

            var json = JsonConvert.SerializeObject(projectGroup);

            File.WriteAllText(@$"{folderPath}\{fileName}.ctmpl", json);

            return Task.FromResult(TaskReturnKind.Exit);
        }

        private Task<(bool, Project?)> GetProjectTask(Solution solution)
        {
            Dictionary<string, Project?> projects = new Dictionary<string, Project?>();
            int index = 1;
            foreach (Project project in solution.Projects)
            {
                Console.WriteLine($"{index}: {project.Name}");
                projects[index.ToString()] = project;
                index++;
            }

            string projectIndex = Console.ReadLine();
            if (!projects.ContainsKey(projectIndex))
            {
                Console.WriteLine("Wrong input try again!");
                return Task.FromResult<(bool, Project)>((false, (Project?)null));
            }
            Project? seletedProject = projects[projectIndex];

            return Task.FromResult<(bool, Project seletedProject)>((true, seletedProject));
        }

        private Task<(TaskReturnKind, DocumentGroup)> TagDocument(DocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(documentGroup.Text);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Text to be tagged.");
            var textPice = Console.ReadLine();

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Input tag name.");
            Console.WriteLine(Environment.NewLine);
            var tag = Console.ReadLine();

            string text = documentGroup.Text.ToString().Replace(textPice, $"${tag}$");

            var tags = documentGroup.TextTags ?? new List<TagElement>();
            tags.Add(new TagElement(tag));

            var newDocumentGroup = new DocumentGroup(documentGroup.Folder, documentGroup.Name, text, documentGroup.FolderTags, documentGroup.NameTags, tags);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Continue tagging?");
            Console.WriteLine(Environment.NewLine);
            var response = Console.ReadLine();
            if (response.ToLower() == "y")
                return Task.FromResult((TaskReturnKind.Continue, newDocumentGroup));

            return Task.FromResult((TaskReturnKind.Exit, newDocumentGroup));
        }

        private Task<(TaskReturnKind, DocumentGroup)> TagFolder(DocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(documentGroup.Folder);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Folder to be tagged.");
            string textPice = Console.ReadLine();

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Input tag name.");
            Console.WriteLine(Environment.NewLine);
            string tag = Console.ReadLine();

            string folder = documentGroup.Folder.Replace(textPice, $"${tag}$");
            var tags = documentGroup.FolderTags ?? new List<TagElement>();
            tags.Add(new TagElement(tag));

            var newDocumentGroup = new DocumentGroup(folder, documentGroup.Name, documentGroup.Text, tags, documentGroup.NameTags, documentGroup.TextTags);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Continue tagging?");
            Console.WriteLine(Environment.NewLine);
            var response = Console.ReadLine();
            if (response.ToLower() == "y")
                return Task.FromResult((TaskReturnKind.Continue, newDocumentGroup));

            return Task.FromResult((TaskReturnKind.Exit, newDocumentGroup));
        }

        private Task<(TaskReturnKind, DocumentGroup)> TagName(DocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(documentGroup.Name);
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("File name to be tagged.");
            string textPice = Console.ReadLine();

            Console.WriteLine("Input tag name.");
            Console.WriteLine(Environment.NewLine);
            string tag = Console.ReadLine();

            string name = documentGroup.Name.Replace(textPice, $"${tag}$");
            var tags = documentGroup.NameTags ?? new List<TagElement>();
            tags.Add(new TagElement(tag));

            var newDocumentGroup = new DocumentGroup(documentGroup.Folder, name, documentGroup.Text, documentGroup.FolderTags, tags, documentGroup.TextTags);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Continue tagging?");
            Console.WriteLine(Environment.NewLine);
            var response = Console.ReadLine();
            if (response.ToLower() == "y")
                return Task.FromResult((TaskReturnKind.Continue, newDocumentGroup));

            return Task.FromResult((TaskReturnKind.Exit, newDocumentGroup));
        }
    }
}