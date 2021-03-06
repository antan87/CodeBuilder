﻿using CodeBuilderApp.Tagging;
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
    internal class CreateTemplatesTaskFunction : ITaskFunction
    {
        private async Task<(TaskReturnKind, TagDocumentGroup?)> GetDocumentTask(Project project)
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
                return (TaskReturnKind.Continue, (TagDocumentGroup?)null);
            }
            Document selecteDocument = documents[documentIndex];
            SourceText text = await selecteDocument.GetTextAsync();
            string folder = string.Join("/", selecteDocument.Folders);
            TagDocumentGroup documentGroup = new TagDocumentGroup(folder, selecteDocument.Name, text.ToString());
            if (!string.IsNullOrWhiteSpace(documentGroup.Folder))
                documentGroup = await TaskExecutable.RunTask<TagDocumentGroup>(TagFolder, documentGroup);
            else
                documentGroup = await TaskExecutable.RunTask<TagDocumentGroup>(TagEmptyFolder, documentGroup);

            documentGroup = await TaskExecutable.RunTask<TagDocumentGroup>(TagName, documentGroup);
            documentGroup = await TaskExecutable.RunTask<TagDocumentGroup>(TagDocument, documentGroup);

            return (TaskReturnKind.Exit, documentGroup);
        }

        public string Name => "Create class templates";

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

            Console.WriteLine("Select project:");
            Project? project = await TaskExecutable.RunTask(CommonTaskFunctions.GetProjectTask, solution);
            if (project == null)
                return;

            List<TagDocumentGroup> documentGroups = new List<TagDocumentGroup>();
            await foreach (TagDocumentGroup? documentGroup in TaskExecutable.RunTaskAsyncEnumerable(GetDocumentTask, project))
                if (documentGroup != null)
                    documentGroups.Add(documentGroup);

            var projectGroup = new TagProjectGroup(documentGroups);
            await TaskExecutable.RunTask(CommonTaskFunctions.SaveDocumentsTask, projectGroup);

            workspace.CloseSolution();
        }

        private Task<(TaskReturnKind, TagDocumentGroup)> TagDocument(TagDocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(documentGroup.Text);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Tagg text y/n?");
            string response = Console.ReadLine();
            if (response == "n")
                return Task.FromResult((TaskReturnKind.Exit, documentGroup));

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Text to be tagged.");
            var textPice = Console.ReadLine();

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Input tag name.");
            Console.WriteLine(Environment.NewLine);
            var tag = Console.ReadLine();

            string text = documentGroup.Text.ToString().Replace(textPice, $"${tag}$");

            var tags = documentGroup.TextTags ?? new List<TagElement>();
            tags.Add(new TagElement(tag, text));

            var newDocumentGroup = new TagDocumentGroup(documentGroup.Folder, documentGroup.Name, text, documentGroup.FolderTags, documentGroup.NameTags, tags);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Continue tagging y/n?");
            Console.WriteLine(Environment.NewLine);
            response = Console.ReadLine();
            if (response.ToLower() == "y")
                return Task.FromResult((TaskReturnKind.Continue, newDocumentGroup));

            return Task.FromResult((TaskReturnKind.Exit, newDocumentGroup));
        }

        private Task<(TaskReturnKind, TagDocumentGroup)> TagFolder(TagDocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(documentGroup.Folder);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Tagg folder y/n?");
            string response = Console.ReadLine();
            if (response == "n")
                return Task.FromResult((TaskReturnKind.Exit, documentGroup));

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Folder to be tagged.");
            string textPice = Console.ReadLine();

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Input tag name.");
            Console.WriteLine(Environment.NewLine);
            string tag = Console.ReadLine();

            string folder = documentGroup.Folder.Replace(textPice, $"${tag}$");
            var tags = documentGroup.FolderTags ?? new List<TagElement>();
            tags.Add(new TagElement(tag, folder));

            var newDocumentGroup = new TagDocumentGroup(folder, documentGroup.Name, documentGroup.Text, tags, documentGroup.NameTags, documentGroup.TextTags);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Continue tagging?");
            Console.WriteLine(Environment.NewLine);
            response = Console.ReadLine();
            if (response.ToLower() == "y")
                return Task.FromResult((TaskReturnKind.Continue, newDocumentGroup));

            return Task.FromResult((TaskReturnKind.Exit, newDocumentGroup));
        }

        private Task<(TaskReturnKind, TagDocumentGroup)> TagEmptyFolder(TagDocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Create folder.");
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("1: Create tagg.");
            Console.WriteLine("2: Create name.");
            Console.WriteLine("Any: Exit.");
            string selectedOption = Console.ReadLine();

            switch (selectedOption)
            {
                case "1":
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine("Input tagg.");
                        string tag = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(tag))
                            return Task.FromResult((TaskReturnKind.Continue, documentGroup));

                        string newFolder = $"{(string.IsNullOrWhiteSpace(documentGroup.Folder) ? string.Empty : documentGroup.Folder + @"\") }${tag}$";
                        List<TagElement> newFolderTags = documentGroup.FolderTags;
                        newFolderTags.Add(new TagElement(tag, newFolder));

                        return Task.FromResult((TaskReturnKind.Continue, new TagDocumentGroup(newFolder, documentGroup.Name, documentGroup.Text, newFolderTags, documentGroup.NameTags, documentGroup.TextTags)));
                    }

                case "2":
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine("Input name.");
                        string name = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(name))
                            return Task.FromResult((TaskReturnKind.Continue, documentGroup));

                        string newFolder = @$"{documentGroup.Folder}\{name}";

                        return Task.FromResult((TaskReturnKind.Continue, new TagDocumentGroup(newFolder, documentGroup.Name, documentGroup.Text, documentGroup.FolderTags, documentGroup.NameTags, documentGroup.TextTags)));
                    }

                default:
                    return Task.FromResult((TaskReturnKind.Exit, documentGroup));
            }
        }

        private Task<(TaskReturnKind, TagDocumentGroup)> TagName(TagDocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(documentGroup.Name);
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Tagg name y/n?");
            string response = Console.ReadLine();
            if (response == "n")
                return Task.FromResult((TaskReturnKind.Exit, documentGroup));

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("File name to be tagged.");
            string textPice = Console.ReadLine();

            Console.WriteLine("Input tag name.");
            Console.WriteLine(Environment.NewLine);
            string tag = Console.ReadLine();

            string name = documentGroup.Name.Replace(textPice, $"${tag}$");
            documentGroup.NameTags.Add(new TagElement(tag, name));

            var newDocumentGroup = new TagDocumentGroup(documentGroup.Folder, name, documentGroup.Text, documentGroup.FolderTags, documentGroup.NameTags, documentGroup.TextTags);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Continue tagging y/n?");
            Console.WriteLine(Environment.NewLine);
            response = Console.ReadLine();
            if (response.ToLower() == "y")
                return Task.FromResult((TaskReturnKind.Continue, newDocumentGroup));

            return Task.FromResult((TaskReturnKind.Exit, newDocumentGroup));
        }
    }
}