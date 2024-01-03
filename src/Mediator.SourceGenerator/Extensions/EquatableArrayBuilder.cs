using System;
using System.Buffers;
using System.Collections.Immutable;

namespace Zapto.Mediator.Generator;

internal ref struct EquatableArrayBuilder<T> where T : IEquatable<T>
{
    private IMemoryOwner<T>? _memory;
    private int _index;

    public EquatableArrayBuilder()
    {
    }

    public EquatableArrayBuilder(int capacity)
    {
        _memory = MemoryPool<T>.Shared.Rent(capacity);
    }

    public int Count => _index;

    public void EnsureCapacity(int capacity)
    {
        if (_memory is null)
        {
            _memory = MemoryPool<T>.Shared.Rent(capacity);
        }
        else if (_memory.Memory.Length < capacity)
        {
            var oldMemory = _memory;

            _memory = MemoryPool<T>.Shared.Rent(capacity);
            oldMemory.Memory.CopyTo(_memory.Memory);
            oldMemory.Memory.Span.Clear();

            oldMemory.Dispose();
        }
    }

    public void Add(T item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        EnsureCapacity(_index + 1);
        _memory!.Memory.Span[_index++] = item;
    }

    public EquatableArray<T> ToEquatableArray()
    {
        return _memory is null ? default : new EquatableArray<T>(_memory.Memory.Slice(0, Count).ToArray());
    }

    public void Dispose()
    {
        _memory?.Memory.Span.Clear();
        _memory?.Dispose();
    }
}
