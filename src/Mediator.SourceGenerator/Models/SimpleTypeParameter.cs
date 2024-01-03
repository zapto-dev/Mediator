using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Zapto.Mediator.Generator;

internal record SimpleTypeParameter(
    string Name,
    EquatableArray<SimpleType> ConstraintTypes,
    ushort SpecialTypeShort,
    bool HasReferenceTypeConstraint,
    bool HasValueTypeConstraint,
    bool HasUnmanagedTypeConstraint,
    bool HasConstructorConstraint
)
{
    public SpecialType SpecialType => (SpecialType) SpecialTypeShort;

    public static EquatableArray<SimpleTypeParameter> FromArray(ImmutableArray<ITypeParameterSymbol> symbolTypeParameters)
    {
        var builder = ImmutableArray.CreateBuilder<SimpleTypeParameter>(symbolTypeParameters.Length);

        foreach (var symbolTypeParameter in symbolTypeParameters)
        {
            builder.Add(FromSymbol(symbolTypeParameter));
        }

        return new EquatableArray<SimpleTypeParameter>(builder.MoveToImmutable());
    }

    private static SimpleTypeParameter FromSymbol(ITypeParameterSymbol parameter)
    {
        return new SimpleTypeParameter(
            parameter.Name,
            SimpleType.FromArray(parameter.ConstraintTypes),
            (ushort)parameter.SpecialType,
            parameter.HasReferenceTypeConstraint,
            parameter.HasValueTypeConstraint,
            parameter.HasUnmanagedTypeConstraint,
            parameter.HasConstructorConstraint
        );
    }
}