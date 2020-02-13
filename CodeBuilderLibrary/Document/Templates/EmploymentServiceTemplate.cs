using CodeBuilderLibrary.Document.Containers;
using CodeBuilderLibrary.Document.Elements;
using System.Collections.Generic;

namespace CodeBuilderLibrary.Document.Templates
{
    internal class EmploymentServiceTemplate
    {
        public EmploymentServiceTemplate(string solutionPath, string modelName, string modelNamePlural)
        {
            this.SolutionPath = solutionPath;
            this.ModelName = modelName;
            this.ModelNamePlural = modelNamePlural;
        }

        public IEnumerable<AddDocumentContainer> GetTemplateContainers(IEnumerable<PropertyElement> properties)
        {
            yield return this.AddModel(properties);

            ////var attribute = new AttributeElement("Key");
            ////var attributeElement = ConstructAttribute.Create(attribute);
            //var property1 = new PropertyElement("public", "string", "Id", true, true);
            //var property1Syntax = ConstructProperty.Create(property1);
            ////property1Syntax = property1Syntax.AddAttributeLists(attributeElement);
            //var property2 = new PropertyElement("public", "string", "EmplyomentPeriodId", true, true);
            //var property2Syntax = ConstructProperty.Create(property2);
            //classSyntax = classSyntax.AddMembers(property1Syntax, property2Syntax);

            //var namespaceElement = new NamespaceElement($"{project.Name}.Folder2");
            //var namespaceSyntax = ConstructNamespace.Create(namespaceElement);
            //namespaceSyntax = namespaceSyntax.AddMembers(classSyntax);
            //syntaxFactory = syntaxFactory.AddMembers(namespaceSyntax);

            //var addedDocument = project.AddDocument("Test3.cs", syntaxFactory.NormalizeWhitespace().ToFullString());
            //var options = addedDocument.Project.CompilationOptions;
            //var changes = await addedDocument.Project.GetCompilationAsync();
            //var diagnostics = changes.GetDeclarationDiagnostics();
            //var diagnostics2 = changes.GetDiagnostics();
            //workspace.TryApplyChanges(addedDocument.Project.Solution);
        }

        private AddDocumentContainer AddModel(IEnumerable<PropertyElement> properties)
        {
            string projectName = "TestApp";
            List<UsingElement> usingElements = new List<UsingElement>()
            {
                new UsingElement("System.ComponentModel.DataAnnotations")
            };

            var namespaceElement = new NamespaceElement($"{projectName}");
            var classElement = new ClassElement(this.ModelName, "public", properties);
            var documentElement = new DocumentElement(usingElements, namespaceElement, classElement);

            return new AddDocumentContainer(projectName, this.ModelName, documentElement);
        }

        private string ModelName { get; }
        private string ModelNamePlural { get; }
        private string SolutionPath { get; }
    }
}