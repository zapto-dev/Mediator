using System;
using System.Text;

namespace Zapto.Mediator.Generator;

internal class IndentedStringBuilder
{
    private static readonly string[] IndentCache = new string[16];

    static IndentedStringBuilder()
    {
        for (var i = 0; i < IndentCache.Length; i++)
        {
            IndentCache[i] = new string(' ', i * 4);
        }
    }

    private int _level;
    private string _indent = string.Empty;
    private bool _newLine = true;

    private int Level
    {
        get => _level;
        set
        {
            if (_level == value)
            {
                return;
            }

            _level = value;
            _indent = IndentCache[value];
        }
    }

    private StringBuilder Builder { get; }

    public int Length
    {
        get => Builder.Length;
        set => Builder.Length = value;
    }

    public char this[int index]
    {
        get => Builder[index];
        set => Builder[index] = value;
    }

    public IndentedStringBuilder(int level = 0)
    {
        Level = level;
        Builder = new StringBuilder();
    }

    public IndentedStringBuilder AppendLine()
    {
        return AppendLine(Environment.NewLine);
    }

    private void WriteIndentIfNeeded()
    {
        if (_newLine)
        {
            Builder.Append(_indent);
            _newLine = false;
        }
    }

    public IndentedStringBuilder Append(char c)
    {
        WriteIndentIfNeeded();
        Builder.Append(c);
        return this;
    }

    public IndentedStringBuilder Append(string? text)
    {
        if (text is null or "")
        {
            return this;
        }

        if (text.HasNewLine())
        {
            foreach (var line in text.GetLines())
            {
                AppendLine(line);
            }
        }
        else
        {
            WriteIndentIfNeeded();
            Builder.Append(text);
        }

        return this;
    }

    public IndentedStringBuilder AppendLine(string text)
    {
        if (text.HasNewLine())
        {
            Append(text);
            Builder.AppendLine();
        }
        else
        {
            WriteIndentIfNeeded();
            Builder.AppendLine(text);
        }

        _newLine = true;
        return this;
    }

    public IndentedStringBuilder AppendLine(ReadOnlySpan<char> text)
    {
        WriteIndentIfNeeded();

        var offset = Builder.Length;

        Builder.Length += text.Length;

        for (var i = 0; i < text.Length; i++)
        {
            Builder[offset + i] = text[i];
        }

        Builder.AppendLine();
        _newLine = true;

        return this;
    }

    public IndentedStringBuilder Indent()
    {
        Level++;
        return this;
    }

    public IndentedStringBuilder Dedent()
    {
        Level--;
        return this;
    }

    /// <summary>
    /// Write a new scope and take a lambda to write to the builder within it. This way it is easy to ensure the
    /// scope is closed correctly.
    /// </summary>
    public DisposeCodeBlock CodeBlock(string openingLine = "", string open = "{", string close = "}")
    {
        if (!string.IsNullOrEmpty(openingLine))
        {
            AppendLine(openingLine);
            AppendLine(open);
        }
        else
        {
            AppendLine(open);
        }

        Indent();
        return new DisposeCodeBlock(close, this);
    }

    public override string ToString()
    {
        return Builder.ToString();
    }

    public readonly struct DisposeCodeBlock(string close, IndentedStringBuilder builder) : IDisposable
    {
        public void Dispose()
        {
            builder.Dedent();
            builder.AppendLine(close);
        }
    }

    public void Insert(int index, string s)
    {
        Builder.Insert(index, s);
    }
}
