using CodeBuilderLibrary.Document.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CodeBuilderLibrary.Document.Constructors
{
    public static class ConstructAttribute
    {
        public static AttributeListSyntax Create(IAttribute element)
        {
            NameSyntax name = SyntaxFactory.ParseName(element.Name);
            AttributeArgumentListSyntax arguments = null;
            if (element.Arguments?.Any() ?? false)
                arguments = SyntaxFactory.ParseAttributeArgumentList($"({string.Join(", ", element.Arguments)})");

            var attribute = SyntaxFactory.Attribute(name, arguments);

            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            attributeList = attributeList.Add(attribute);

            return SyntaxFactory.AttributeList(attributeList);
        }
    }
}