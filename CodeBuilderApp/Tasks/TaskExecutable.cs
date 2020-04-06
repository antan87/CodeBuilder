using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeBuilderApp.Tasks
{
    public static class TaskExecutable
    {
        internal static async Task<T?> RunTask<T, K>(Func<K, Task<(bool, T?)>> task, K input) where T : class
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

        internal static async IAsyncEnumerable<T?> RunTaskAsyncEnumerable<T, K>(Func<K, Task<(TaskReturnKind, T?)>> task, K input) where T : class
        {
            bool exit = false;
            while (!exit)
            {
                (TaskReturnKind, T?) result = await task(input);
                if (result.Item1 == TaskReturnKind.Exit)
                    exit = true;

                if (result.Item2 != null)
                    yield return result.Item2;
            }
        }

        internal static async IAsyncEnumerable<T?> RunTaskAsyncEnumerable<T>(Func<Task<(TaskReturnKind, T?)>> task) where T : class
        {
            bool exit = false;
            while (!exit)
            {
                (TaskReturnKind, T?) result = await task();
                if (result.Item1 == TaskReturnKind.Exit)
                    exit = true;

                if (result.Item2 != null)
                    yield return result.Item2;
            }
        }

        internal static async Task<T> RunTask<T>(Func<T, T, Task<(TaskReturnKind, T)>> task, T input, T input2) where T : class
        {
            bool exit = false;
            while (!exit)
            {
                (TaskReturnKind, T) result = await task(input, input2);
                if (result.Item1 == TaskReturnKind.Exit)
                    exit = true;

                input = result.Item2;
            }

            return input;
        }

        internal static async Task<T> RunTask<T>(Func<T, Task<(TaskReturnKind, T)>> task, T input) where T : class
        {
            bool exit = false;
            while (!exit)
            {
                (TaskReturnKind, T) result = await task(input);
                if (result.Item1 == TaskReturnKind.Exit)
                    exit = true;

                input = result.Item2;
            }

            return input;
        }

        internal static async Task<T?> RunTask<T>(Func<Task<(TaskReturnKind, T?)>> task) where T : class
        {
            bool exit = false;
            T? output = default;
            while (!exit)
            {
                (TaskReturnKind, T?) result = await task();
                if (result.Item1 == TaskReturnKind.Exit)
                    exit = true;

                output = result.Item2;
            }

            return output;
        }

        internal static async Task<T?> RunTask<T>(Func<Task<(TaskReturnKind, T?)>> task) where T : struct
        {
            bool exit = false;
            T? output = default;
            while (!exit)
            {
                (TaskReturnKind, T?) result = await task();
                if (result.Item1 == TaskReturnKind.Exit)
                    exit = true;

                output = result.Item2;
            }

            return output;
        }

        internal static async Task RunTask<T>(Func<T, Task<TaskReturnKind>> task, T input) where T : class
        {
            bool exit = false;
            while (!exit)
            {
                TaskReturnKind result = await task(input);
                if (result == TaskReturnKind.Exit)
                    exit = true;
            }
        }

        internal static async Task<T> RunTask<T>(this T input, Func<T, Task<(TaskReturnKind, T)>> task) where T : class
        {
            bool exit = false;
            while (!exit)
            {
                (TaskReturnKind, T) result = await task(input);
                if (result.Item1 == TaskReturnKind.Exit)
                    exit = true;

                input = result.Item2;
            }

            return input;
        }
    }
}