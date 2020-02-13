using CodeBuilderLibrary.Document.Elements;
using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Interfaces
{
    public interface IClass
    {
        IEnumerable<IAttribute> Attributes { get; }
        string ModifierDeclaration { get; }
        string Name { get; }
        IEnumerable<PropertyElement> Properties { get; }
    }
}