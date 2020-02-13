using CodeBuilderLibrary.Document.Interfaces;

namespace CodeBuilderLibrary.Document.Elements
{
    public sealed class NamespaceElement : INamespace
    {
        public NamespaceElement(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}