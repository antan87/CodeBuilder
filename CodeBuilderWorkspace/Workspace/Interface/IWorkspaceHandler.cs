namespace CodeBuilderWorkspace.Workspace.Interface
{
    public interface IWorkspaceHandler<TWorkspace>
    where TWorkspace : Microsoft.CodeAnalysis.Workspace

    {
        TWorkspace Workspace { get; }
    }
}