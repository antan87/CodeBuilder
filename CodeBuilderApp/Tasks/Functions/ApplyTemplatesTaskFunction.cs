using CodeBuilderApp.Common;
using CodeBuilderApp.Tagging;
using CodeBuilderApp.Tasks.Interfaces;
using CodeBuilderWorkspace.Workspace.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeBuilderApp.Tasks.Functions
{
    public sealed class ApplyTemplatesTaskFunction : ITaskFunction
    {
        public string Name => "Apply templates";

        private async Task<(TaskReturnKind, TagDocumentGroup)> ReplaceDocumentTagsTask(TagDocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Implement tags for file {documentGroup.Name}");
            Console.WriteLine(Environment.NewLine);

            if (documentGroup.FolderTags.Any())
                documentGroup = await TaskExecutable.RunTask<TagDocumentGroup>(this.ReplaceFolderTags, documentGroup);

            if (documentGroup.NameTags.Any())
                documentGroup = await TaskExecutable.RunTask<TagDocumentGroup>(this.ReplaceNameTags, documentGroup);

            if (documentGroup.TextTags.Any())
                documentGroup = await TaskExecutable.RunTask<TagDocumentGroup>(this.ReplaceTextTags, documentGroup);

            return (TaskReturnKind.Exit, documentGroup);
        }

        private async Task<(TaskReturnKind, TagDocumentGroup)> ReplaceFolderTags(TagDocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Replace folder tags: {documentGroup.Folder}");
            Console.WriteLine(Environment.NewLine);

            foreach (var tag in documentGroup.FolderTags)
            {
                var replacedFolderText = await TaskExecutable.RunTask(this.ReplaceTagTask, tag.Tag, documentGroup.Folder);
                documentGroup = new TagDocumentGroup(replacedFolderText, documentGroup.Name, documentGroup.Text, documentGroup.FolderTags, documentGroup.NameTags, documentGroup.TextTags);
            }

            return (TaskReturnKind.Exit, documentGroup);
        }

        private async Task<(TaskReturnKind, TagDocumentGroup)> ReplaceNameTags(TagDocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Replace name tags: {documentGroup.Name}");
            Console.WriteLine(Environment.NewLine);

            foreach (var tag in documentGroup.NameTags)
            {
                string replacedNameText = await TaskExecutable.RunTask(this.ReplaceTagTask, tag.Tag, documentGroup.Name);
                documentGroup = new TagDocumentGroup(documentGroup.Folder, replacedNameText, documentGroup.Text, documentGroup.FolderTags, documentGroup.NameTags, documentGroup.TextTags);
            }

            return (TaskReturnKind.Exit, documentGroup);
        }

        private async Task<(TaskReturnKind, TagDocumentGroup)> ReplaceTextTags(TagDocumentGroup documentGroup)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Replace text tags: {documentGroup.Text}");
            Console.WriteLine(Environment.NewLine);

            foreach (var tag in documentGroup.TextTags)
            {
                string? replacedText = await TaskExecutable.RunTask(this.ReplaceTagTask, tag.Tag, documentGroup.Text);
                documentGroup = new TagDocumentGroup(documentGroup.Folder, documentGroup.Name, replacedText, documentGroup.FolderTags, documentGroup.NameTags, documentGroup.TextTags);
            }

            return (TaskReturnKind.Exit, documentGroup);
        }

        private Task<(TaskReturnKind, string)> ReplaceTagTask(string tag, string content)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Replace tag: {tag}");
            Console.WriteLine(Environment.NewLine);
            string newText = Console.ReadLine();
            Console.WriteLine($"Tag {tag} replaced with {newText}?");
            Console.WriteLine(Environment.NewLine);
            string response = Console.ReadLine();

            if (response.ToLower() == "y")
            {
                string newContent = content.Replace(@$"${tag}$", newText);
                return Task.FromResult((TaskReturnKind.Exit, newContent));
            }

            return Task.FromResult((TaskReturnKind.Continue, content));
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

            await foreach (var test in TaskExecutable.RunTaskAsyncEnumerable(this.SelectProjectTask, solution))
            {
                if (test != null)
                    workspace.TryApplyChanges(test.Project.Solution);
            }

            workspace.CloseSolution();
        }

        private async Task<(TaskReturnKind, Document?)> SelectProjectTask(Solution solution)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select project:");
            Project? project = await TaskExecutable.RunTask(CommonTaskFunctions.GetProjectTask, solution);
            if (project == null)
                return (TaskReturnKind.Continue, default);

            await foreach (TagProjectGroup? projectGroup in TaskExecutable.RunTaskAsyncEnumerable(this.SelectTemplateFileTask, project))
            {
                if (projectGroup == null)
                    continue;

                foreach (TagDocumentGroup documentGroup in projectGroup.Documents)
                {
                    TagDocumentGroup newDocumentGroup = await TaskExecutable.RunTask<TagDocumentGroup>(this.ReplaceDocumentTagsTask, documentGroup);
                    Document? newDocument = this.AppendDocumentToProject(project, newDocumentGroup);
                    return (TaskReturnKind.Continue, newDocument);
                }
            }

            return (TaskReturnKind.Exit, default);
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
    }
}