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

        public ProjectGroup(IList<DocumentGroup> documents, IEnumerable<TagElement> tags)
        {
            this.Documents = documents;
            this.Tags = tags;
        }

        private ProjectGroup()
        {
        }

        public IEnumerable<TagElement> Tags { get; } = new List<TagElement>();
    }
}