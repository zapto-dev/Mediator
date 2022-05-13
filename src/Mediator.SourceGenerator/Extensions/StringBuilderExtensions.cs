using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Zapto.Mediator.Generator;

public enum AppendResult
{
    None,
    Type,
    TypeAndName
}

public static class StringBuilderExtensions
{
    public static string GetNamespace(this ITypeSymbol? symbol)
    {
        var ns = symbol?.ContainingNamespace;

        if (ns is null or { Name: null or null })
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        AppendNamespace(sb, ns.ContainingNamespace);
        sb.Append(ns.Name);
        return sb.ToString();
    }

    public static void AppendNamespace(this StringBuilder sb, INamespaceSymbol type)
    {
        if (type is {Name: null or ""})
        {
            return;
        }

        AppendNamespace(sb, type.ContainingNamespace);
        sb.Append(type.Name);
        sb.Append('.');
    }

    public static void AppendValue(this StringBuilder sb, object? value)
    {
        sb.Append(value switch
        {
            null => "default",
            string s => '"' + s.Replace("\"", "\\\"") + '"',
            bool b => b ? "true" : "false",
            IConvertible c => c.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString()
        });
    }

    public static void AppendType(
        this StringBuilder sb,
        ITypeSymbol type,
        bool addNullable = true,
        Func<ITypeSymbol, bool>? middleware = null,
        bool addGenericNames = true)
    {
        if (type is not {SpecialType: SpecialType.None})
        {
            sb.Append(type.ToDisplayString());
            return;
        }

        if (middleware?.Invoke(type) ?? false)
        {
            return;
        }

        if (type is ITypeParameterSymbol)
        {
            sb.Append(type.Name);
            addNullable = false;
        }
        else
        {
            sb.Append("global::");
            AppendNamespace(sb, type.ContainingNamespace);
            sb.Append(type.Name);
        }

        if (type is INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
        {
            sb.Append('<');

            var length = namedTypeSymbol.TypeArguments.Length;
            for (var i = 0; i < length; i++)
            {
                if (addGenericNames)
                {
                    AppendType(sb, namedTypeSymbol.TypeArguments[i], false, middleware);

                    if (i != length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                else if (i != length - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append('>');
        }

        if (addNullable &&
            !type.IsValueType &&
            type.Name is not ("Nullable" or "ValueTask") &&
            type.NullableAnnotation is NullableAnnotation.Annotated or NullableAnnotation.None)
        {
            sb.Append('?');
        }
    }

    public static void AppendParameters(
        this StringBuilder sb,
        IReadOnlyList<IParameterSymbol> symbols,
        Func<IParameterSymbol, bool>? middleware = null)
    {
        var length = symbols.Count;
        for (var i = 0; i < length; i++)
        {
            var parameter = symbols[i];

            if (middleware?.Invoke(parameter) is false or null)
            {
                sb.Append(parameter.Name);
            }

            if (i != length - 1)
            {
                sb.Append(", ");
            }
        }
    }

    public static void AppendParameterDefinitions(
        this StringBuilder sb,
        IReadOnlyList<IParameterSymbol> symbols,
        Func<IParameterSymbol, AppendResult>? middleware = null)
    {
        var length = symbols.Count;

        for (var i = 0; i < length; i++)
        {
            var parameter = symbols[i];
            var result = middleware?.Invoke(parameter) ?? AppendResult.None;

            if (result is AppendResult.None)
            {
                sb.AppendType(parameter.Type);
            }

            if (result is AppendResult.None or AppendResult.Type)
            {
                sb.Append(' ');
                sb.Append(parameter.Name);

                if (parameter.HasExplicitDefaultValue)
                {
                    sb.Append(" = ");
                    sb.AppendValue(parameter.ExplicitDefaultValue);
                }
            }

            var len = sb.Length;

            if (i != length - 1 && (len <= 2 || sb[len - 2] != ',' && sb[len - 1] != ' '))
            {
                sb.Append(", ");
            }
        }
    }

}
