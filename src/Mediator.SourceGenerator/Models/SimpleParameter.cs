using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Zapto.Mediator.Generator;

internal record SimpleParameter(
    string Name,
    SimpleType Type,
    bool HasExplicitDefaultValue,
    object? ExplicitDefaultValue
)
{
    public static EquatableArray<SimpleParameter> FromArray(ImmutableArray<IParameterSymbol> symbolParameters)
    {
        var builder = ImmutableArray.CreateBuilder<SimpleParameter>(symbolParameters.Length);

        foreach (var symbolParameter in symbolParameters)
        {
            builder.Add(FromSymbol(symbolParameter));
        }

        return new EquatableArray<SimpleParameter>(builder.MoveToImmutable());
    }

    private static SimpleParameter FromSymbol(IParameterSymbol parameter)
    {
        return new SimpleParameter(
            parameter.Name,
            SimpleType.FromSymbol(parameter.Type),
            parameter.HasExplicitDefaultValue,
            parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null
        );
    }

}