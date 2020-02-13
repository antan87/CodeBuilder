using CodeBuilderLibrary.Document.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeBuilderLibrary.Document.Constructors
{
    public static class ConstructNamespace
    {
        public static NamespaceDeclarationSyntax Create(INamespace element) => SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(element.Name)).NormalizeWhitespace();
    }
}