using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Project = Microsoft.CodeAnalysis.Project;

namespace CodeBuilderWorkspace.Workspace.Interface
{
    public interface IWorkspaceHandler<TWorkspace>
    where TWorkspace : Microsoft.CodeAnalysis.Workspace

    {
        TWorkspace Workspace { get; }

        Task<Solution> GetSolution(string solutionPath);

        Task<Project> GetProject(string projectFilePath);
    }
}