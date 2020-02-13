using CodeBuilderLibrary.Document.Interfaces;

namespace CodeBuilderLibrary.Document.Elements
{
    public sealed class UsingElement : IUsing
    {
        public UsingElement(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}