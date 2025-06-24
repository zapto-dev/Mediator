using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
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

        return ns?.ToDisplayString() ?? "";
    }

    internal static void AppendValue(this IndentedStringBuilder sb, object? value)
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

    internal static void AppendType(
        this IndentedStringBuilder sb,
        SimpleType type,
        bool addNullable = true,
        Func<SimpleType, bool>? middleware = null,
        bool addGenericNames = true)
    {
        if (type is not {SpecialType: SpecialType.None})
        {
            sb.Append(type.ToDisplayString());
        }
        else
        {
            if (middleware?.Invoke(type) ?? false)
            {
                return;
            }

            if (type.IsTypeParameter)
            {
                sb.Append(type.Name);
                addNullable = false;
            }
            else
            {
                sb.Append("global::");

                if (type.Namespace is not null)
                {
                    sb.Append(type.Namespace);
                    sb.Append('.');
                }

                sb.Append(type.Name);
            }

            if (type.IsGenericType)
            {
                sb.Append('<');

                var length = type.TypeArguments.Length;
                for (var i = 0; i < length; i++)
                {
                    if (addGenericNames)
                    {
                        AppendType(sb, type.TypeArguments[i], addNullable, middleware);

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
        }

        if (addNullable &&
            !type.IsValueType &&
            type.Name is not ("Nullable" or "ValueTask") &&
            type.NullableAnnotation is NullableAnnotation.Annotated)
        {
            sb.Append('?');
        }
    }

    internal static void AppendParameters(
        this IndentedStringBuilder sb,
        EquatableArray<SimpleParameter> symbols,
        Func<SimpleParameter, bool>? middleware = null)
    {
        var length = symbols.Length;
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

    internal static void AppendParameterDefinitions<T>(
        this IndentedStringBuilder sb,
        T symbols,
        Func<SimpleParameter, AppendResult>? middleware = null)
        where T : IReadOnlyList<SimpleParameter>
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

        if (sb.Length > 2 && sb[sb.Length - 2] == ',' && sb[sb.Length - 1] == ' ')
        {
            sb.Length -= 2;
        }
    }

    internal static void AppendGenericConstraints(this IndentedStringBuilder sb, SimpleType type)
    {
        if (!type.IsGenericType)
        {
            return;
        }

        foreach (var parameter in type.TypeParameters)
        {
            var valid = parameter.ConstraintTypes.Length > 0 || parameter.HasReferenceTypeConstraint ||
                        parameter.HasValueTypeConstraint ||
                        parameter.HasUnmanagedTypeConstraint || parameter.HasConstructorConstraint;

            if (!valid)
            {
                continue;
            }

            sb.Append("where ");
            sb.Append(parameter.Name);
            sb.Append(" : ");

            int j;
            for (j = 0; j < parameter.ConstraintTypes.Length; j++)
            {
                if (j > 1) sb.Append(", ");
                sb.AppendType(parameter.ConstraintTypes[j], false);
            }

            if (parameter.HasReferenceTypeConstraint)
            {
                if (j++ > 1) sb.Append(", ");
                sb.Append("class");
            }
            else if (parameter.HasValueTypeConstraint)
            {
                if (j++ > 1) sb.Append(", ");
                sb.Append("struct");
            }
            else if (parameter.HasUnmanagedTypeConstraint)
            {
                if (j++ > 1) sb.Append(", ");
                sb.Append("unmanaged");
            }

            if (parameter.HasConstructorConstraint)
            {
                if (j > 1) sb.Append(", ");
                sb.Append("new()");
            }

            sb.AppendLine();
        }
    }

}
