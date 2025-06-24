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

    public static EquatableArray<SimpleParameter> GetRequiredProperties(INamedTypeSymbol type)
    {
        var builder = ImmutableArray.CreateBuilder<SimpleParameter>();

        foreach (var symbol in type.GetMembers())
        {
            if (symbol is not IPropertySymbol property)
            {
                continue;
            }

            var isRequired = property.IsRequired || HasRequiredAttribute(property);

            if (!isRequired || property.IsStatic || property.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            builder.Add(new SimpleParameter(
                property.Name,
                SimpleType.FromSymbol(property.Type),
                false,
                null
            ));
        }

        return new EquatableArray<SimpleParameter>(builder.ToImmutable());
    }

    private static bool HasRequiredAttribute(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass?.Name == "RequiredAttribute" &&
                attribute.AttributeClass.ContainingNamespace.ToDisplayString() == "System.ComponentModel.DataAnnotations")
            {
                return true;
            }
        }
        return false;
    }
}