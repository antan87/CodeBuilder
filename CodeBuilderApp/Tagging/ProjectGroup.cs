using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class ProjectGroup
    {
        public ProjectGroup(IList<DocumentGroup> documents)
        {
            this.Documents = documents;
        }

        public IList<DocumentGroup> Documents { get; }
    }
}