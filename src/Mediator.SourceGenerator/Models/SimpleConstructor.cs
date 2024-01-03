using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Zapto.Mediator.Generator;

internal record SimpleConstructor(
    byte DeclaredAccessibilityByte,
    EquatableArray<SimpleParameter> Parameters)
{
    public Accessibility DeclaredAccessibility => (Accessibility) DeclaredAccessibilityByte;

    public static EquatableArray<SimpleConstructor> FromArray(ImmutableArray<IMethodSymbol> constructors)
    {
        var builder = new EquatableArrayBuilder<SimpleConstructor>(constructors.Length);

        foreach (var constructor in constructors)
        {
            builder.Add(FromSymbol(constructor));
        }

        return builder.ToEquatableArray();
    }

    private static SimpleConstructor FromSymbol(IMethodSymbol constructor)
    {
        return new SimpleConstructor(
            (byte)constructor.DeclaredAccessibility,
            SimpleParameter.FromArray(constructor.Parameters)
        );
    }
}
