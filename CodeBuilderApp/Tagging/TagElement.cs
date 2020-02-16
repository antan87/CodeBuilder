namespace CodeBuilderApp.Tagging
{
    public sealed class TagElement
    {
        public TagElement(string tag)
        {
            this.Tag = tag;
        }

        public string Tag { get; }
    }
}