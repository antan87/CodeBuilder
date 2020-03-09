namespace CodeBuilderApp.Tagging
{
    public sealed class TagElement
    {
        public TagElement(string tag, string replaceText)
        {
            this.Tag = tag;
            this.ReplaceText = replaceText;
        }

        public string Tag { get; }
        public string ReplaceText { get; }
    }
}