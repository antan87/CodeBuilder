using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace CodeBuilderApp.Tagging
{
    public sealed class CreateProjectGroup
    {
        public CreateProjectGroup(Project project, List<Document> documents)
        {
            this.Project = project;
            this.Documents= documents;
        }

        public Project Project { get; }
        public List<Document> Documents { get; } = new List<Document>();
    }
}