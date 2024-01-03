using System;

namespace Zapto.Mediator.Generator;

public static class SpanExtensions
{
    public static SpanSplitLinesEnumerator SplitLines(this ReadOnlySpan<char> span)
    {
        return new SpanSplitLinesEnumerator(span);
    }

    public ref struct SpanSplitLinesEnumerator(ReadOnlySpan<char> remaining)
    {
        private ReadOnlySpan<char> _remaining = remaining;

        public SpanSplitLinesEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var span = _remaining;
            if (span.Length == 0)
            {
                return false;
            }

            var index = span.IndexOfAny('\r', '\n');

            if (index == -1)
            {
                _remaining = ReadOnlySpan<char>.Empty;
                Current = span;
                return true;
            }

            if (span[index] == '\r' && index + 1 < span.Length && span[index + 1] == '\n')
            {
                index++;
            }

            Current = span.Slice(0, index);
            _remaining = span.Slice(index + 1);
            return true;
        }

        public ReadOnlySpan<char> Current { get; private set; } = default;
    }
}
