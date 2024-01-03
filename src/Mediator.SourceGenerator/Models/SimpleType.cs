using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Zapto.Mediator.Generator;

internal record SimpleType(
    byte DeclaredAccessibilityByte,
    byte NullableAnnotationByte,
    sbyte SpecialTypeByte,
    string Name,
    bool IsGenericType,
    bool IsTypeParameter,
    bool IsValueType,
    string? Namespace,
    string ContainingAssembly,
    EquatableArray<SimpleTypeParameter> TypeParameters,
    EquatableArray<SimpleType> TypeArguments,
    EquatableArray<SimpleConstructor> Constructors
)
{
    public string UniqueId => Namespace is null ? Name : $"{Namespace.Replace('.', '_')}_{Name}";

    public Accessibility DeclaredAccessibility => (Accessibility) DeclaredAccessibilityByte;

    public NullableAnnotation NullableAnnotation => (NullableAnnotation) NullableAnnotationByte;

    public SpecialType SpecialType => (SpecialType) SpecialTypeByte;

    public static EquatableArray<SimpleType> FromArray(ImmutableArray<ITypeSymbol> symbolTypeParameters)
    {
        var builder = new EquatableArrayBuilder<SimpleType>(symbolTypeParameters.Length);

        foreach (var symbolTypeParameter in symbolTypeParameters)
        {
            builder.Add(FromSymbol(symbolTypeParameter));
        }

        return builder.ToEquatableArray();
    }

    public static SimpleType FromSymbol(ITypeSymbol symbol, bool withConstructors = false)
    {
        if (symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return new SimpleType(
                (byte)symbol.DeclaredAccessibility,
                (byte)symbol.NullableAnnotation,
                (sbyte)symbol.SpecialType,
                symbol.Name,
                false,
                symbol is ITypeParameterSymbol,
                symbol.IsValueType,
                symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.GetNamespace(),
                symbol.ContainingAssembly.Name,
                EquatableArray<SimpleTypeParameter>.Empty,
                EquatableArray<SimpleType>.Empty,
                EquatableArray<SimpleConstructor>.Empty
            );
        }

        return new SimpleType(
            (byte)namedTypeSymbol.DeclaredAccessibility,
            (byte)namedTypeSymbol.NullableAnnotation,
            (sbyte)namedTypeSymbol.SpecialType,
            namedTypeSymbol.Name,
            namedTypeSymbol.IsGenericType,
            false,
            namedTypeSymbol.IsValueType,
            namedTypeSymbol.ContainingNamespace.IsGlobalNamespace ? null : namedTypeSymbol.GetNamespace(),
            namedTypeSymbol.ContainingAssembly.Name,
            SimpleTypeParameter.FromArray(namedTypeSymbol.TypeParameters),
            FromArray(namedTypeSymbol.TypeArguments),
            withConstructors? SimpleConstructor.FromArray(namedTypeSymbol.Constructors) : EquatableArray<SimpleConstructor>.Empty
        );
    }

    public string ToDisplayString()
    {
        return SpecialType switch
        {
            SpecialType.System_Object => "object",
            SpecialType.System_Enum => "enum",
            SpecialType.System_Void => "void",
            SpecialType.System_Boolean => "bool",
            SpecialType.System_Char => "char",
            SpecialType.System_SByte => "sbyte",
            SpecialType.System_Byte => "byte",
            SpecialType.System_Int16 => "short",
            SpecialType.System_UInt16 => "ushort",
            SpecialType.System_Int32 => "int",
            SpecialType.System_UInt32 => "uint",
            SpecialType.System_Int64 => "long",
            SpecialType.System_UInt64 => "ulong",
            SpecialType.System_Decimal => "decimal",
            SpecialType.System_Single => "float",
            SpecialType.System_Double => "double",
            SpecialType.System_String => "string",
            SpecialType.System_IntPtr => "nint",
            SpecialType.System_UIntPtr => "nuint",
            SpecialType.System_Array => $"{Namespace}.{Name}[]",
            _ when Namespace is not null && IsGenericType => $"{Namespace}.{Name}<{string.Join(", ", TypeArguments)}>",
            _ when Namespace is not null => $"{Namespace}.{Name}",
            _ when IsGenericType => $"{Name}<{string.Join(", ", TypeArguments)}>",
            _ => Name,
        };
    }

    public override string ToString() => ToDisplayString();

    public virtual bool SimpleEquals(SimpleType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Namespace == other.Namespace && ContainingAssembly == other.ContainingAssembly && TypeParameters.Equals(other.TypeParameters);
    }
}