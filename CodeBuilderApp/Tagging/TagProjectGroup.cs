using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class TagProjectGroup
    {
        public IList<TagDocumentGroup> Documents { get; } = new List<TagDocumentGroup>();

        public TagProjectGroup(IList<TagDocumentGroup> documents)
        {
            this.Documents = documents;
        }

        public TagProjectGroup(IList<TagDocumentGroup> documents, IEnumerable<TagElement> tags)
        {
            this.Documents = documents;
            this.Tags = tags;
        }

        private TagProjectGroup()
        {
        }

        public IEnumerable<TagElement> Tags { get; } = new List<TagElement>();
    }
}