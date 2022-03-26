using MediatR;

namespace Mediator.DependencyInjection.Tests.Generics;

public record struct ReturnGenericRequest<TValue>(TValue Value) : IRequest<TValue>;