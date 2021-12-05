using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
namespace AttributesSourceGeneratorHelper;
internal static class Extensions
{
    public static bool HasClass(this ClassDeclarationSyntax source, string interfaceName)
    {
        if (source.BaseList is null)
        {
            return false;
        }
        IEnumerable<BaseTypeSyntax> baseTypes = source.BaseList.Types.Select(baseType => baseType);
        return baseTypes.Any(baseType => baseType.ToString() == interfaceName);
    }
    public static bool HasClass(this ITypeSymbol symbol, string interfaceName)
    {
        var firsts = symbol.AllInterfaces;
        return firsts.Any(xx => xx.Name == interfaceName);
    }
}