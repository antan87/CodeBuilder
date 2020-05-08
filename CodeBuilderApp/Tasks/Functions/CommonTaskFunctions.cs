using CodeBuilderApp.Common;
using CodeBuilderApp.Extensions;
using CodeBuilderApp.Tagging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeBuilderApp.Tasks.Functions
{
    public static class CommonTaskFunctions
    {
        public static Document? AppendDocumentToProject(Project project, TagDocumentGroup documentGroup)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentGroup.Text);
            if (syntaxTree.TryGetRoot(out SyntaxNode node))
                return project.AddDocument(documentGroup.Name, node, documentGroup.Folder.Split(@"\"));

            return null;
        }

        public static Task<(bool, Project?)> GetProjectTask(Solution solution)
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
                return Task.FromResult<(bool, Project?)>((false, default));
            }
            Project? seletedProject = projects[projectIndex];

            return Task.FromResult((true, seletedProject));
        }

        public static async Task<TagProjectGroup> ImplementsTags(IEnumerable<Document> documents, IEnumerable<TagElement> tags)
        {
            List<TagDocumentGroup> documentGroups = new List<TagDocumentGroup>();
            foreach (var document in documents)
            {
                var taggedDocument = await TagDocument(document, tags);
                documentGroups.Add(taggedDocument);
            }

            return new TagProjectGroup(documentGroups, tags);
        }

        public static TagDocumentGroup ReplaceDocumentTags(TagDocumentGroup documentGroup, IEnumerable<ReplacedTagElement> tags)
        {
            documentGroup = CommonTaskFunctions.ReplaceFolderTags(documentGroup, tags);
            documentGroup = CommonTaskFunctions.ReplaceNameTags(documentGroup, tags);
            documentGroup = CommonTaskFunctions.ReplaceTextTags(documentGroup, tags);

            return documentGroup;
        }

        private static TagDocumentGroup ReplaceFolderTags(TagDocumentGroup documentGroup, IEnumerable<ReplacedTagElement> tags)
        {
            foreach (ReplacedTagElement tag in tags)
            {
                string replacedFolderText = documentGroup.Folder.ReplaceTagWithText(tag.NewContent, tag.Tag);
                documentGroup = new TagDocumentGroup(replacedFolderText, documentGroup.Name, documentGroup.Text);
            }

            return documentGroup;
        }

        private static TagDocumentGroup ReplaceNameTags(TagDocumentGroup documentGroup, IEnumerable<ReplacedTagElement> tags)
        {
            foreach (ReplacedTagElement tag in tags)
            {
                string replacedNameText = documentGroup.Name.ReplaceTagWithText(tag.NewContent, tag.Tag);
                documentGroup = new TagDocumentGroup(documentGroup.Folder, replacedNameText, documentGroup.Text);
            }

            return documentGroup;
        }

        private static TagDocumentGroup ReplaceTextTags(TagDocumentGroup documentGroup, IEnumerable<ReplacedTagElement> tags)
        {
            foreach (ReplacedTagElement tag in tags)
            {
                string replaceText = documentGroup.Text.ReplaceTagWithText(tag.NewContent, tag.Tag);
                documentGroup = new TagDocumentGroup(documentGroup.Folder, documentGroup.Name, replaceText);
            }

            return documentGroup;
        }

        public static Task<TaskReturnKind> SaveDocumentsTask(TagProjectGroup projectGroup)
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

            string json = JsonConvert.SerializeObject(projectGroup);

            File.WriteAllText(@$"{folderPath}\{fileName}{FileExtensions.FileTemplateExtension}", json);

            return Task.FromResult(TaskReturnKind.Exit);
        }

        private static Task<(TaskReturnKind, Document?)> SelectDocument(IEnumerable<Document> projectDocuments)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select document:");
            Dictionary<string, Document> documents = new Dictionary<string, Document>();
            int index = 1;
            foreach (Document document in projectDocuments)
            {
                Console.WriteLine($"{index}: {(document.Folders.Any() ? string.Join(@"\", document.Folders) + @"\" : string.Empty)}{document.Name}");

                documents[index.ToString()] = document;
                index++;
            }

            Console.WriteLine(Environment.NewLine);
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

        public async static Task<IEnumerable<Document>> SelectDocuments(Project project)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select documents to be tagged:");
            Console.WriteLine(Environment.NewLine);

            List<Document> selectedDocuments = new List<Document>();
            DocumentSearchOption? documentOption = await TaskExecutable.RunTask(CommonTaskFunctions.SelectDocumentSearchOption);
            if (!documentOption.HasValue)
                return selectedDocuments;

            switch (documentOption.Value)
            {
                case DocumentSearchOption.Search:
                    var fetchedDocuments = await TaskExecutable.RunTaskEnumerable<Document>(CommonTaskFunctions.SearchAndSelectDocuments, project.Documents);

                    foreach (var document in fetchedDocuments)
                        selectedDocuments.Add(document);

                    break;

                case DocumentSearchOption.Select:
                    await foreach (Document? document in TaskExecutable.RunTaskAsyncEnumerable(CommonTaskFunctions.SelectDocument, project.Documents.Where(whereDocument => !selectedDocuments.Contains(whereDocument))))
                    {
                        if (document != null)
                            selectedDocuments.Add(document);
                    }
                    break;
            }

            return selectedDocuments;
        }

        private static Task<(TaskReturnKind, DocumentSearchOption?)> SelectDocumentSearchOption()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select search option:");
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine($"{(int)DocumentSearchOption.Search}: Search and select multiple documents.");
            Console.WriteLine($"{(int)DocumentSearchOption.Select}: Select single document.");
            Console.WriteLine(Environment.NewLine);

            string selectedOption = Console.ReadLine();
            DocumentSearchOption? enumOption = Enum.Parse<DocumentSearchOption>(selectedOption);
            if (!enumOption.HasValue)
            {
                Console.WriteLine("Wrong input!");
                Console.WriteLine(Environment.NewLine);
                return Task.FromResult((TaskReturnKind.Continue, (DocumentSearchOption?)default));
            }

            return Task.FromResult((TaskReturnKind.Exit, enumOption));
        }

        public static Task<(TaskReturnKind, IEnumerable<Document>)> SearchAndSelectDocuments(IEnumerable<Document> projectDocuments)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Enter wildcard value to be searched:");
            Console.WriteLine(Environment.NewLine);

            string searchValue = Console.ReadLine();

            IEnumerable<Document> matchedDocuments = projectDocuments.Where(document => document.Name.ToLower().Contains(searchValue.ToLower()));
            if (!matchedDocuments.Any())
            {
                Console.WriteLine("Couldn't find any documents that matched the input wildcard try again!");
                return Task.FromResult((TaskReturnKind.Continue, Enumerable.Empty<Document>()));
            }

            Dictionary<string, Document> documentDictionary = new Dictionary<string, Document>();
            int index = 1;
            foreach (Document document in matchedDocuments)
            {
                Console.WriteLine($"{index}: {(document.Folders.Any() ? string.Join(@"\", document.Folders) + @"\" : string.Empty)}{document.Name}");

                documentDictionary[index.ToString()] = document;
                index++;
            }

            Console.WriteLine($"A: All documents");
            Console.WriteLine(Environment.NewLine);

            string selectedIndexs = Console.ReadLine();

            if ("a" == selectedIndexs.ToLower().Trim())
                return Task.FromResult((TaskReturnKind.Exit, matchedDocuments));

            string[] documentIndexs = selectedIndexs.Split(",");
            if (!documentDictionary.Any(document => documentIndexs.Contains(document.Key)))
            {
                Console.WriteLine("Wrong input try again!");
                return Task.FromResult((TaskReturnKind.Continue, Enumerable.Empty<Document>()));
            }

            Console.WriteLine(Environment.NewLine);

            IEnumerable<Document> documents = documentDictionary.
                Where(document => documentIndexs.Contains(document.Key)).
                Select(document => document.Value);

            return Task.FromResult((TaskReturnKind.Exit, documents));
        }

        private static async Task<TagDocumentGroup> TagDocument(Document document, IEnumerable<TagElement> tags)
        {
            string folder = string.Join("/", document.Folders);
            string name = document.Name;
            SourceText sourceText = await document.GetTextAsync();
            string text = sourceText.ToString();
            var documentGroup = new TagDocumentGroup(folder, name, text);

            foreach (TagElement tag in tags)
            {
                string newFolderText = documentGroup.Folder.ReplaceTextWithTag(tag.ReplaceText, tag.Tag);
                string newNameText = documentGroup.Name.ReplaceTextWithTag(tag.ReplaceText, tag.Tag);
                string newText = documentGroup.Text.ReplaceTextWithTag(tag.ReplaceText, tag.Tag);

                documentGroup = new TagDocumentGroup(newFolderText, newNameText, newText);
            }

            return documentGroup;
        }

        private enum DocumentSearchOption
        {
            Search = 0,
            Select = 1
        }
    }
}