using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class CreateDocumentGroup
    {
        public CreateDocumentGroup(IEnumerable<ReplacedTagElement> replacedTags)
        {
            this.ReplacedTags = replacedTags;
        }

        public IEnumerable<ReplacedTagElement> ReplacedTags { get; } = new List<ReplacedTagElement>();
    }
}