using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class DocumentGroup
    {
        public DocumentGroup(string name, string text, List<TagElement> tags)
        {
            this.Name = name;
            this.Text = text;
            this.Tags = tags;
        }

        public DocumentGroup(string name, string text)
        {
            this.Name = name;
            this.Text = text;
        }

        public string Name { get; }
        public List<TagElement>? Tags { get; }
        public string Text { get; }
    }
}