using CodeBuilderLibrary.Document.Interfaces;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CodeBuilderLibrary.Document.Constructors
{
    public static class ConstructClass
    {
        public static ClassDeclarationSyntax Create(IClass element)
        {
            var modifierDeclaration = SyntaxFactory.ParseToken(element.ModifierDeclaration);
            var classDeclaration = SyntaxFactory.ClassDeclaration(element.Name).AddModifiers(modifierDeclaration);

            if (element.Attributes?.Any() ?? false)
                classDeclaration = classDeclaration.AddAttributeLists(element.Attributes.Select(item => ConstructAttribute.Create(item)).ToArray());

            return classDeclaration;
        }
    }
}