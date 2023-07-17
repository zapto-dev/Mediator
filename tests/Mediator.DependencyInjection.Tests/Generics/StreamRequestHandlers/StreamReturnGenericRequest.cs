using MediatR;

namespace Mediator.DependencyInjection.Tests.Generics;

public record struct ReturnGenericStreamRequest<TValue>(TValue Value) : IStreamRequest<TValue>;