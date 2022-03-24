using System;

namespace Zapto.Mediator;

public readonly record struct MediatorNamespace(string Value)
{
    public static explicit operator MediatorNamespace(string value) => new(value);
};
