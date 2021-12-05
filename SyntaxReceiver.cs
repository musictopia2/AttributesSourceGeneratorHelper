using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
/// <summary>
/// Created on demand before each generation pass
/// </summary>
namespace AttributesSourceGeneratorHelper;
internal class SyntaxReceiver : ISyntaxReceiver
{
    public IList<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();
    /// <summary>
    /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
    /// </summary>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
            classDeclarationSyntax.HasClass("Attribute")
            )
        {
            CandidateClasses.Add(classDeclarationSyntax);
        }
    }
}