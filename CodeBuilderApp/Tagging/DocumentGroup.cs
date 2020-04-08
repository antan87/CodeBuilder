using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class DocumentGroup
    {
        public DocumentGroup(IEnumerable<ReplacedTagElement> replacedTags)
        {
            this.ReplacedTags = replacedTags;
        }

        public IEnumerable<ReplacedTagElement> ReplacedTags { get; } = new List<ReplacedTagElement>();
    }
}