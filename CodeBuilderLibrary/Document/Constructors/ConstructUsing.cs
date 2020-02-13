using CodeBuilderLibrary.Document.Interfaces;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeBuilderLibrary.Document.Constructors
{
    public static class ConstructUsing
    {
        public static UsingDirectiveSyntax Create(IUsing element) => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(element.Name));
    }
}