using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Interfaces
{
    public interface IProperty
    {
        public IEnumerable<IAttribute> Attributes { get; }
        public bool GetAccessor { get; }
        public string ModifierDeclaration { get; }
        public string Name { get; }
        public bool SetAccessor { get; }
        public string Type { get; }
    }
}