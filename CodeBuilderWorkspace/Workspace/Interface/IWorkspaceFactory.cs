using System.Threading.Tasks;

namespace CodeBuilderWorkspace.Workspace.Interface
{
    public interface IWorkspaceFactory<TWorkspace>
    where TWorkspace : Microsoft.CodeAnalysis.Workspace
    {
        Task<TWorkspace> GetWorkspace(string solutionPath);
    }
}