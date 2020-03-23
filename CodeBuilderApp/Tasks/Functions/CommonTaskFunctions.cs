using CodeBuilderApp.Common;
using CodeBuilderApp.Tagging;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CodeBuilderApp.Tasks.Functions
{
    public static class CommonTaskFunctions
    {
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

        public static Task<TaskReturnKind> SaveDocumentsTask(ProjectGroup projectGroup)
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
    }
}