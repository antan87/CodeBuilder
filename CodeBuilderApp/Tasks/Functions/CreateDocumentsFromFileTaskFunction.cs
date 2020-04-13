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
    internal class CreateDocumentsFromFileTaskFunction : ITaskFunction
    {
        public string Name => "Create documents from file";

        public async Task RunTask()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(this.Name);
            Console.WriteLine(Environment.NewLine);

            TagTemplateGroup? template = await TaskExecutable.RunTask(this.LoadTemplate);
            if (template == null)
                return;

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

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Select project:");
            Project? project = await TaskExecutable.RunTask(CommonTaskFunctions.GetProjectTask, solution);
            if (project == null)
                return;

            IEnumerable<Document> selectedDocuments = await CommonTaskFunctions.SelectDocuments(project);
            TagProjectGroup projectGroup = await CommonTaskFunctions.ImplementsTags(selectedDocuments, template.TagElements);
            List<Document> documents = new List<Document>();
            foreach (DocumentGroup createDocument in template.Documents)
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

            workspace.TryApplyChanges(project.Solution);
            workspace.CloseSolution();
        }

        private async Task<(TaskReturnKind, TagTemplateGroup?)> LoadTemplate()
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
                TagTemplateGroup template = JsonConvert.DeserializeObject<TagTemplateGroup>(json);
                return (TaskReturnKind.Exit, template);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong!!");
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(e.Message);

                return (TaskReturnKind.Continue, default);
            }
        }
    }
}