using CodeBuilderWorkspace.Workspace.Interface;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Threading.Tasks;

namespace CodeBuilderWorkspace.Workspace.Factory
{
    public class MSBuildWorkspaceFactory : IWorkspaceFactory<MSBuildWorkspace>
    {
        public Task<MSBuildWorkspace> GetWorkspace()
        {
            MSBuildLocator.RegisterDefaults();
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

            return Task.FromResult(workspace);
        }
    }
}