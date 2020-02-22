namespace CodeBuilderApp.Tasks.Interfaces
{
    public interface ITaskFunction
    {
        System.Threading.Tasks.Task RunTask();

        string Name { get; }
    }
}