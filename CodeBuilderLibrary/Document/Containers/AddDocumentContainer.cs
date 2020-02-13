using CodeBuilderLibrary.Document.Elements;
using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Containers
{
    public sealed class AddDocumentContainer
    {
        public AddDocumentContainer(string projectName, string name, DocumentElement document, IEnumerable<string> folders = default)
        {
            this.Document = document;
            this.ProjectName = projectName;
            this.Name = name;
            this.Folders = folders;
        }

        public DocumentElement Document { get; }

        public IEnumerable<string> Folders { get; }
        public string Name { get; }
        public string ProjectName { get; }
    }
}