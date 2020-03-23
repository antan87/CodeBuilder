using CodeBuilderApp.Tasks.Functions;
using CodeBuilderApp.Tasks.Interfaces;
using System;
using System.Collections.Generic;

namespace CodeBuilderApp
{
    public sealed class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            await new Program().MainConsole();
        }

        private async System.Threading.Tasks.Task MainConsole()
        {
            Console.WriteLine("Welcome select a option.");
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Options");
                foreach (var task in this.Tasks)
                    Console.WriteLine($"{task.Key}: {task.Value.Name}");

                string taskIndex = Console.ReadLine();
                if (!this.Tasks.ContainsKey(taskIndex))
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine("Wrong input try again!");
                    continue;
                }
                ITaskFunction function = this.Tasks[taskIndex];

                await function.RunTask();

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Continue Y/N?");
                string continueRespone = Console.ReadLine();
                if (continueRespone.ToLower() == "n")
                    exit = true;
            }
        }

        private readonly Dictionary<string, ITaskFunction> Tasks = new Dictionary<string, ITaskFunction>()
        {
            ["1"] = new CreateTemplatesTaskFunctionV2(),
            ["2"] = new ApplyTemplatesTaskFunctionV2()
        };
    }
}