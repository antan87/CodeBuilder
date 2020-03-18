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
using System.Threading.Tasks;

namespace CodeBuilderApp.Tasks.Functions
{
    public sealed class ApplyTemplatesTaskFunctionV2 : ITaskFunction
    {
        public string Name => "Apply templates";

        private DocumentGroup ReplaceDocumentTags(DocumentGroup documentGroup, IEnumerable<(string tag, string newContent)> tags)
        {
            documentGroup = this.ReplaceFolderTags(documentGroup, tags);
            documentGroup = this.ReplaceNameTags(documentGroup, tags);
            documentGroup = this.ReplaceTextTags(documentGroup, tags);

            return documentGroup;
        }

        private DocumentGroup ReplaceFolderTags(DocumentGroup documentGroup, IEnumerable<(string tag, string newContent)> tags)
        {
            foreach ((string tag, string newContent) tag in tags)
            {
                string replacedFolderText = documentGroup.Folder.ReplaceTagWithText(tag.newContent, tag.tag);
                documentGroup = new DocumentGroup(replacedFolderText, documentGroup.Name, documentGroup.Text);
            }

            return documentGroup;
        }

        private DocumentGroup ReplaceNameTags(DocumentGroup documentGroup, IEnumerable<(string tag, string newContent)> tags)
        {
            foreach ((string tag, string newContent) tag in tags)
            {
                string replacedNameText = documentGroup.Name.ReplaceTagWithText(tag.newContent, tag.tag);
                documentGroup = new DocumentGroup(documentGroup.Folder, replacedNameText, documentGroup.Text);
            }

            return documentGroup;
        }

        private DocumentGroup ReplaceTextTags(DocumentGroup documentGroup, IEnumerable<(string tag, string newContent)> tags)
        {
            foreach ((string tag, string newContent) tag in tags)
            {
                string replaceText = documentGroup.Text.ReplaceTagWithText(tag.newContent, tag.tag);
                documentGroup = new DocumentGroup(documentGroup.Folder, documentGroup.Name, replaceText);
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

            await foreach (var result in TaskExecutable.RunTask(this.SelectProjectTask, solution))
            {
                if (result == null || result.Project == null)
                    continue;

                workspace.TryApplyChanges(result.Project.Solution);
            }

            workspace.CloseSolution();
        }

        private async Task<(TaskReturnKind, Test?)> SelectProjectTask(Solution solution)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select project:");
            Project? project = await TaskExecutable.RunTask(CommonTaskFunctions.GetProjectTask, solution);
            if (project == null)
                return (TaskReturnKind.Continue, null);

            await foreach (ProjectGroup? projectGroup in TaskExecutable.RunTask(this.SelectTemplateFileTask, project))
            {
                if (projectGroup == null)
                    continue;

                IEnumerable<(string tag, string newContent)> replacedTags = this.ReplaceTags(projectGroup.Tags);

                List<Document> documents = new List<Document>();
                foreach (DocumentGroup documentGroup in projectGroup.Documents)
                {
                    DocumentGroup newDocumentGroup = this.ReplaceDocumentTags(documentGroup, replacedTags);
                    Document? newDocument = this.AppendDocumentToProject(project, newDocumentGroup);
                    if (newDocument != null)
                    {
                        project = newDocument.Project;
                        documents.Add(newDocument);
                    }
                }

                var ss = new Test { Project = project, Documents = documents };
                return (TaskReturnKind.Continue, ss);
            }

            return (TaskReturnKind.Exit, null);
        }

        private List<(string tag, string newContent)> ReplaceTags(IEnumerable<TagElement> tagElements)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Replace tags:");

            var list = new List<(string tag, string newContent)>();
            foreach (TagElement element in tagElements)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"Tag: {element.Tag} ");
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"Input text to replace tag with.");
                string newContent = Console.ReadLine();
                list.Add((element.Tag, newContent));
            }

            return list;
        }

        private Document? AppendDocumentToProject(Project project, DocumentGroup documentGroup)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentGroup.Text);
            if (syntaxTree.TryGetRoot(out SyntaxNode node))
                return project.AddDocument(documentGroup.Name, node, documentGroup.Folder.Split(@"\"));

            return null;
        }

        private async Task<(TaskReturnKind, ProjectGroup?)> SelectTemplateFileTask(Project project)
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

            ProjectGroup projectGroup = JsonConvert.DeserializeObject<ProjectGroup>(json);

            return (TaskReturnKind.Exit, projectGroup);
        }

        private sealed class Test
        {
            public Project? Project { get; set; }
            public List<Document> Documents { get; set; } = new List<Document>();
        }
    }
}