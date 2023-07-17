using MediatR;

namespace Mediator.DependencyInjection.Tests.Generics;

public record struct ReturnNullableGenericStreamRequest<TValue>(TValue Value) : IStreamRequest<TValue?>
    where TValue : struct;