using CodeBuilderLibrary.Document.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CodeBuilderLibrary.Document.Constructors
{
    public static class ConstructDocument
    {
        public static string Create(IDocument element)
        {
            CompilationUnitSyntax syntaxFactory = SyntaxFactory.CompilationUnit();
            var classDeclarations = element.Classes.Select(item => ConstructClass.Create(item));
            var namespaceDeclaration = ConstructNamespace.Create(element.Namespace);
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclarations.ToArray());

            var usingDeclarations = element.Usings.Select(item => ConstructUsing.Create(item));
            namespaceDeclaration = namespaceDeclaration.AddUsings(usingDeclarations.ToArray());

            return syntaxFactory.NormalizeWhitespace().ToFullString();
        }
    }
}