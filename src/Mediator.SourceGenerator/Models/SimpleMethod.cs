using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Zapto.Mediator.Generator;

internal record SimpleMethod(
    string Name,
    bool IsGenericMethod,
    SimpleType ReturnType,
    EquatableArray<SimpleParameter> Parameters,
    EquatableArray<SimpleTypeParameter> TypeParameters
)
{
    public static EquatableArray<SimpleMethod> FromArray(ImmutableArray<IMethodSymbol> symbolMethods)
    {
        var builder = ImmutableArray.CreateBuilder<SimpleMethod>(symbolMethods.Length);

        foreach (var symbolMethod in symbolMethods)
        {
            builder.Add(FromSymbol(symbolMethod));
        }

        return new EquatableArray<SimpleMethod>(builder.MoveToImmutable());
    }

    public static SimpleMethod FromSymbol(IMethodSymbol symbol)
    {
        return new SimpleMethod(
            symbol.Name,
            symbol.IsGenericMethod,
            SimpleType.FromSymbol(symbol.ReturnType),
            SimpleParameter.FromArray(symbol.Parameters),
            SimpleTypeParameter.FromArray(symbol.TypeParameters)
        );
    }
}