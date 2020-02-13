using CodeBuilderLibrary.Document.Interfaces;
using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Elements
{
    public class AttributeElement : IAttribute
    {
        public AttributeElement(string name, IEnumerable<string> arguments = null)
        {
            this.Name = name;
            this.Arguments = arguments;
        }

        public IEnumerable<string> Arguments { get; }

        public string Name { get; }
    }
}