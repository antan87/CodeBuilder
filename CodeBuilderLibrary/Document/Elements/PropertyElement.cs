using CodeBuilderLibrary.Document.Interfaces;
using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Elements
{
    public sealed class PropertyElement : IProperty
    {
        public PropertyElement(string modifierDeclaration, string type, string name, bool getAccessor = true, bool setAccessor = true, IEnumerable<IAttribute> attributes = default)
        {
            this.Attributes = attributes;
            this.ModifierDeclaration = modifierDeclaration;
            this.Type = type;
            this.Name = name;
            this.GetAccessor = getAccessor;
            this.SetAccessor = setAccessor;
        }

        public IEnumerable<IAttribute> Attributes { get; }
        public bool GetAccessor { get; }
        public string ModifierDeclaration { get; }
        public string Name { get; }
        public bool SetAccessor { get; }
        public string Type { get; }
    }
}