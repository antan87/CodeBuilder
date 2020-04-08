using System.Collections.Generic;
using System.Linq;

namespace CodeBuilderApp.Tagging
{
    public sealed class TagTemplateGroup
    {
        public TagTemplateGroup(IEnumerable<TagElement> tagElements, IEnumerable<DocumentGroup> documents)
        {
            this.TagElements = tagElements;
            this.Documents = documents;
        }

        public IEnumerable<DocumentGroup> Documents { get; } = Enumerable.Empty<DocumentGroup>();

        public IEnumerable<TagElement> TagElements { get; } = Enumerable.Empty<TagElement>();
    }
}