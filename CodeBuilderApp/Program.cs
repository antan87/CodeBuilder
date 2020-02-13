using CodeBuilderWorkspace.Workspace.Factory;
using CodeBuilderWorkspace.Workspace.Handler;
using System.Threading.Tasks;

namespace CodeBuilderApp
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var factory = new MSBuildWorkspaceFactory();
            var workspace = await factory.GetWorkspace(@"C:\Users\Antan87\source\repos\C#\Test\TestApp\TestApp.sln");
            var handler = new MSBuildWorkspaceHandler(workspace);
        }
    }
}