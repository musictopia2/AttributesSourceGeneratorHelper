using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.Misc;
using CommonBasicLibraries.CollectionClasses;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
namespace AttributesSourceGeneratorHelper;
[Generator] //this is important so it knows this class is a generator which will generate code for a class using it.
public class MySourceGenerator : ISourceGenerator
{
    private BasicList<string> GetList<T>(BasicList<T> list, Func<T, string> field)
    {
        return list.Select(field).ToBasicList();
    }
    private BasicList<string> GetAttributeProperties(BasicList<IPropertySymbol> properties, string className)
    {

        int upto = 0;
        int index;
        BasicList<string> output = new();
        foreach (var property in properties)
        {
            index = -1;
            if (property.IsRequiredAttributeUsed())
            {
                index = upto;
                upto++;
            }
            StringBuilder builder = new();
            output.Add($@"    public static AttributeProperty Get{property.Name}Info => new(""{property.Name}"", {index});");
            builder.AppendLine($@"    public static string {property.Name} => ""{property.Name}"";");
            output.Add(builder.ToString());
        }
        string shorts = className.Replace("Attribute", "");
        output.Add($@"    public static string {className} => ""{shorts}"";");
        return output;
    }
    private ClassInfo GetClass(string name, string content)
    {
        string propertyCode;
        propertyCode = $@"    private const string _code{name} = @""{content}"";";
        string compilation = $"        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(_code{name}, Encoding.UTF8), options));";
        string source = $@"        context.AddSource(""{name}"", SourceText.From(_code{name}, Encoding.UTF8));";
        return new(propertyCode, compilation, source);
    }
    private string GetConstructors(ClassDeclarationSyntax clazz, Compilation compilation)
    {
        var ss = compilation.GetClassSymbol(clazz);
        var properties = ss.GetRequiredProperties();
        if (properties.Count == 0)
        {
            return "";
        }
        StringBuilder builder = new();
        StrCat cats = new();
        string name = clazz.Identifier.ValueText;
        cats.AddToString($"    public {name}(");
        properties.ForEach(p =>
        {
            string firsts = p.Name;
            string vType = p.Type.ToDisplayString();
            string parameter = firsts.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            cats.AddToString($"{vType} {parameter}", ", ");

        });
        cats.AddToString(")");
        cats.AddToString(Environment.NewLine);
        cats.AddToString("    {");
        properties.ForEach(p =>
        {
            string firsts = p.Name;
            string parameter = firsts.ChangeCasingForVariable(EnumVariableCategory.ParameterCamelCase);
            cats.AddToString($"        {firsts} = {parameter};", Environment.NewLine);
        });
        cats.AddToString(Environment.NewLine);
        cats.AddToString("    }");
        string results = cats.GetInfo();
        results = results.Replace("(, ", "("); //unfortunately had to do this part to remove the beginning , part.
        return results;
    }
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }
        Compilation compilation = context.Compilation;
        BasicList<ClassInfo> list = new();
        string source;
        foreach (var ourClass in receiver.CandidateClasses)
        {
            string className = ourClass.Identifier.ValueText;
            string content = ourClass.SyntaxTree.ToString();

            string extras = GetConstructors(ourClass, compilation);
            if (string.IsNullOrWhiteSpace(extras) == false)
            {
                content = ourClass.AppendCodeText(content, extras);
            }
            content = content.GetCSharpString();
            content = content.RemoveAttribute("Required"); //i think this too.  hopefully this simple.
            INamedTypeSymbol symbol = compilation.GetClassSymbol(ourClass);
            var properties = symbol.GetProperties();
            string shortName = className.Replace("Attribute", "");
            list.Add(GetClass(shortName, content));
           
            //needs some other source though.
            //because each of the classes needs its own helpers to get the information needed.
            //otherwise, can have conflicts (which is bad).
            BasicList<string> helps = GetAttributeProperties(properties, className);
            source = @$"namespace {compilation.AssemblyName}.AttributeHelpers;
internal static class {shortName}
{{
{string.Join(Environment.NewLine, helps)}
}}
";
            context.AddSource($"{shortName}.g", source);
        }
        source = @$"global using aa = {compilation.AssemblyName}.AttributeHelpers;
namespace {compilation.AssemblyName};
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
internal static class Extensions
{{
{string.Join(Environment.NewLine, GetList(list, xx => xx.Code))}
    internal static Compilation GetCompilationWithAttributes(this GeneratorExecutionContext context)
    {{
{string.Join(Environment.NewLine, GetList(list, xx => xx.AddSource))}
        var options = context.Compilation.SyntaxTrees.First().Options as CSharpParseOptions;
        Compilation compilation = context.Compilation;
{string.Join(Environment.NewLine, GetList(list, xx => xx.Compilation))}
        return compilation;
    }}
}}";
        context.AddSource("Extensions.g", source);
    }
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}