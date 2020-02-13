using CodeBuilderLibrary.Document.Interfaces;
using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Elements
{
    public sealed class ClassElement : IClass
    {
        public ClassElement(string name, string modifierDeclaration, IEnumerable<PropertyElement> properties = default, IEnumerable<IAttribute> attributes = default)
        {
            this.Attributes = attributes;
            this.Name = name;
            this.ModifierDeclaration = modifierDeclaration;
            this.Properties = properties;
        }

        public IEnumerable<IAttribute> Attributes { get; }
        public string ModifierDeclaration { get; }
        public string Name { get; }
        public IEnumerable<PropertyElement> Properties { get; }
    }
}