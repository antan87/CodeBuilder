using CodeBuilderWorkspace.Workspace.Factory;
using Microsoft.CodeAnalysis;
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

            Console.WriteLine("Select document:");
        }

        private static Task<(bool, Document?)> GetDocumentTask(Project project)
        {
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
                return Task.FromResult((false, (Document?)null));
            }
            Document selecteDocument = documents[documentIndex];

            return Task.FromResult((true, selecteDocument));
        }

        private static Task<(bool, Project?)> GetProjectTask(Solution solution)
        {
            Dictionary<string, Project> projects = new Dictionary<string, Project>();
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
            Project seletedProject = projects[projectIndex];

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

                var selectedTask = this.Tasks[taskIndex];
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
    }
}