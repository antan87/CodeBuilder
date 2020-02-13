using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Interfaces
{
    public interface IDocument
    {
        public IEnumerable<IUsing> Usings { get; }
        public INamespace Namespace { get; }
        public IEnumerable<IClass> Classes { get; }
    }
}