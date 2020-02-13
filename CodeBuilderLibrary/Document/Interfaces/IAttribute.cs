using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Interfaces
{
    public interface IAttribute
    {
        IEnumerable<string> Arguments { get; }
        string Name { get; }
    }
}