using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
namespace AttributesSourceGeneratorHelper;
[Generator] //this is important so it knows this class is a generator which will generate code for a class using it.
public class MySourceGenerator : ISourceGenerator
{
    private List<string> GetList<T>(List<T> list, Func<T, string> field)
    {
        return list.Select(field).ToList();
    }
    private ClassInfo GetClass(string name, string content)
    {
        string propertyCode;
        string propertyName;
        propertyName = $@"    internal const string {name} = ""{name}"";";
        propertyCode = $@"    private const string _code{name} = @""{content}"";";
        string compilation = $"        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(_code{name}, Encoding.UTF8), options));";
        string source = $"        context.AddSource({name}, SourceText.From(_code{name}, Encoding.UTF8));";
        return new(propertyName, propertyCode, compilation, source);
    }
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }
        Compilation compilation = context.Compilation;
        List<ClassInfo> list = new();
        foreach (var ourClass in receiver.CandidateClasses)
        {
            string className = ourClass.Identifier.ValueText;
            string content = ourClass.SyntaxTree.ToString();
            content = content.Replace("\"", "\"\"");
            list.Add(GetClass(className, content));
        }
        string source = @$"using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
namespace {compilation.AssemblyName};
internal class AttributeHelpers
{{
{string.Join(Environment.NewLine, GetList(list, xx => xx.Property))}
{string.Join(Environment.NewLine, GetList(list, xx => xx.Code))}
    internal static Compilation GetCompilation(GeneratorExecutionContext context)
    {{
{string.Join(Environment.NewLine, GetList(list, xx => xx.AddSource))}
        var options = context.Compilation.SyntaxTrees.First().Options as CSharpParseOptions;
        Compilation compilation = context.Compilation;
{string.Join(Environment.NewLine, GetList(list, xx => xx.Compilation))}
        return compilation;
    }}
}}";
        context.AddSource("AttributeHelpers.g", source);
    }
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}