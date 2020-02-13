using CodeBuilderLibrary.Document.Interfaces;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CodeBuilderLibrary.Document.Constructors
{
    public static class ConstructProperty
    {
        public static PropertyDeclarationSyntax Create(IProperty element)
        {
            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(element.Type), element.Name)
                   .AddModifiers(SyntaxFactory.ParseToken(element.ModifierDeclaration));

            if (element.SetAccessor)
                propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            if (element.GetAccessor)
                propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            if (element.Attributes?.Any() ?? false)
                propertyDeclaration = propertyDeclaration.AddAttributeLists(element.Attributes.Select(item => ConstructAttribute.Create(item)).ToArray());

            return propertyDeclaration;
        }
    }
}