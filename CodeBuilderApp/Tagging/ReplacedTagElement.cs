namespace CodeBuilderApp.Tagging
{
    public sealed class ReplacedTagElement
    {
        public ReplacedTagElement(string tag, string newContent)
        {
            this.Tag = tag;
            this.NewContent = newContent;
        }

        public string Tag { get; }
        public string NewContent { get; }
    }
}