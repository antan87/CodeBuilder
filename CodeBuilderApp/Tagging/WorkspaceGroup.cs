using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class WorkspaceGroup
    {
        public WorkspaceGroup(IList<SolutionGroup> solutions)
        {
            this.Solutions = solutions;
        }

        public IList<SolutionGroup> Solutions { get; }
    }
}