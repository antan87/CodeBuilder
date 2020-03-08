using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class ProjectGroup
    {
        public IList<DocumentGroup> Documents { get; } = new List<DocumentGroup>();

        public ProjectGroup(IList<DocumentGroup> documents)
        {
            this.Documents = documents;
        }

        private ProjectGroup()
        {
        }
    }
}