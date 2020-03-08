using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class DocumentGroup
    {
        public DocumentGroup(string folder, string name, string text, List<TagElement> folderTags, List<TagElement> nameTags, List<TagElement> textTags)
        {
            this.Folder = folder;
            this.Name = name;
            this.Text = text;
            this.FolderTags = folderTags;
            this.NameTags = nameTags;
            this.TextTags = textTags;
        }

        public DocumentGroup(string folder, string name, string text)
        {
            this.Folder = folder;
            this.Name = name;
            this.Text = text;
        }

        private DocumentGroup()
        {
        }

        public string Folder { get; set; } = string.Empty;
        public List<TagElement> FolderTags { get; } = new List<TagElement>();
        public string Name { get; set; } = string.Empty;
        public List<TagElement> NameTags { get; } = new List<TagElement>();

        public string Text { get; set; } = string.Empty;
        public List<TagElement> TextTags { get; } = new List<TagElement>();
    }
}