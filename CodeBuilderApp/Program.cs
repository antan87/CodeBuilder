using CodeBuilderApp.Tagging;
using CodeBuilderWorkspace.Workspace.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeBuilderApp
{
    public sealed class Program
    {
        private readonly Dictionary<string, (string text, Func<Task> task)> Tasks = new Dictionary<string, (string text, Func<Task> task)>()
        {
            ["1"] = ("Create class templates", CreateDocumentTemplatesTask)
        };

        private static async Task CreateDocumentTemplatesTask()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Create class templates:");
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Input solution path.");
            Console.WriteLine(Environment.NewLine);
            string solutionPath = Console.ReadLine();
            using var workspace = await new MSBuildWorkspaceFactory().GetWorkspace();
            Solution solution = await workspace.OpenSolutionAsync(solutionPath);

            Console.WriteLine("Select project:");
            Project? project = await RunTask(GetProjectTask, solution);
            if (project == null)
                return;

            Console.WriteLine("Select document:");
            Document? document = await RunTask(GetDocumentTask, project);
            if (document == null)
                return;

            SourceText text = await document.GetTextAsync();
            DocumentGroup documentGroup = new DocumentGroup(document.Name, text.ToString());
            documentGroup = await RunTask<DocumentGroup>(TagDocument, documentGroup);

            workspace.CloseSolution();
        }

        private static Task<(bool, DocumentGroup?)> TagDocument(DocumentGroup? documentGroup)
        {
            Console.WriteLine(documentGroup.Text);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Input tag name.");
            Console.WriteLine(Environment.NewLine);
            var tag = Console.ReadLine();
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Text to be tagged.");
            var textPice = Console.ReadLine();

            string text = documentGroup.Text.ToString().Replace(textPice, $"${tag}$");

            var tags = documentGroup.Tags ?? new List<TagElement>();
            tags.Add(new TagElement(tag));

            var newDocumentGroup = new DocumentGroup(documentGroup.Name, text, tags);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Continue tagging?");
            Console.WriteLine(Environment.NewLine);
            var response = Console.ReadLine();
            if (response.ToLower() == "y")
                return Task.FromResult((true, newDocumentGroup));

            return Task.FromResult((false, newDocumentGroup));
        }

        private static Task<(bool, Document?)> GetDocumentTask(Project project)
        {
            Dictionary<string, Document?> documents = new Dictionary<string, Document?>();
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
                return Task.FromResult((false, (Document?)null));
            }
            Document? selecteDocument = documents[documentIndex];

            return Task.FromResult((true, selecteDocument));
        }

        private static Task<(bool, Project?)> GetProjectTask(Solution solution)
        {
            Dictionary<string, Project?> projects = new Dictionary<string, Project?>();
            int index = 1;
            Console.WriteLine("Select project:");
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
                return Task.FromResult((false, (Project?)null));
            }
            Project? seletedProject = projects[projectIndex];

            return Task.FromResult((true, seletedProject));
        }

        public static async Task Main(string[] args)
        {
            await new Program().MainConsole();
        }

        private async Task MainConsole()
        {
            Console.WriteLine("Welcome select a option.");
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Options");
                foreach (var task in this.Tasks)
                    Console.WriteLine($"{task.Key}: {task.Value.text}");

                string taskIndex = Console.ReadLine();
                if (!this.Tasks.ContainsKey(taskIndex))
                {
                    Console.WriteLine("Wrong input try again!");
                    continue;
                }

                (string text, Func<Task> task) selectedTask = this.Tasks[taskIndex];
                await selectedTask.task();

                Console.WriteLine("Continue Y/N?");
                string continueRespone = Console.ReadLine();
                if (continueRespone.ToLower() == "n")
                    exit = true;
            }
        }

        private static async Task<T?> RunTask<T, K>(Func<K, Task<(bool, T?)>> task, K input)
        where T : class
        {
            T? value = default;
            bool exit = false;
            while (!exit)
            {
                (bool, T?) result = await task(input);
                if (!result.Item1)
                    continue;

                value = result.Item2;
                exit = true;
            }

            return value;
        }

        private static async Task<T?> RunTask<T>(Func<T?, Task<(bool, T?)>> task, T? input)
       where T : class
        {
            T? value = default;
            bool exit = false;
            while (!exit)
            {
                (bool, T?) result = await task(input);
                input = result.Item2;
                if (!result.Item1)
                    continue;

                value = input;
                exit = true;
            }

            return value;
        }

        private static async IAsyncEnumerable<T> RunTask<T, K>(Func<K, Task<(TaskReturnKind, T)>> task, K input)
             where T : class
        {
            bool exit = false;
            while (!exit)
            {
                (TaskReturnKind, T) result = await task(input);
                if (result.Item1 == TaskReturnKind.Exit)
                    exit = true;

                if (result.Item2 != null)
                    yield return result.Item2;
            }
        }

        private enum TaskReturnKind
        {
            Exit = 0,
            Continue = 1,
            Error = 2
        }
    }
}