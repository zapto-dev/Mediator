using System;

namespace Zapto.Mediator.Generator;

public static class StringExtensions
{
    /// <summary>Splits the specified <paramref name="value"/> based on line ending.</summary>
    /// <param name="value">The input string to split.</param>
    /// <returns>An array of each line in the string.</returns>
    public static SpanExtensions.SpanSplitLinesEnumerator GetLines(this string value)
    {
        return string.IsNullOrWhiteSpace(value) ? default : value.AsSpan().SplitLines();
    }

    /// <summary>Verifies if the string contains a new line.</summary>
    /// <param name="value">The input string to check.</param>
    /// <returns>True if the string contains a new line, false otherwise.</returns>
    public static bool HasNewLine(this string value)
    {
        return value.AsSpan().IndexOfAny('\r', '\n') != -1;
    }

    public static string RemoveSuffix(this string name, string suffix)
    {
        if (name.EndsWith(suffix) && name.Length != suffix.Length)
        {
            name = name.Substring(0, name.Length - suffix.Length);
        }

        return name;
    }


    public static string RemovePrefix(this string name, string suffix)
    {
        if (name.StartsWith(suffix) && name.Length != suffix.Length)
        {
            name = name.Substring(suffix.Length, name.Length - suffix.Length);
        }

        return name;
    }
}
