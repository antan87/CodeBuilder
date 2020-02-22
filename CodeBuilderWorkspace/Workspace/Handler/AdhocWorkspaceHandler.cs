using CodeBuilderWorkspace.Workspace.Interface;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace CodeBuilderWorkspace.Workspace.Handler
{
    public sealed class AdhocWorkspaceHandler : IWorkspaceHandler<AdhocWorkspace>
    {
        public AdhocWorkspaceHandler(AdhocWorkspace workspace)
        {
            this.Workspace = workspace;
        }

        public AdhocWorkspace Workspace { get; }

        public Task<Project> GetProject(string projectFilePath)
        {
            throw new System.NotImplementedException();
        }

        public Task<Solution> GetSolution(string solutionPath)
        {
            throw new System.NotImplementedException();
        }
    }
}