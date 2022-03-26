using MediatR;

namespace Mediator.DependencyInjection.Tests.Generics;

public record struct ReturnNullableGenericRequest<TValue>(TValue Value) : IRequest<TValue?>
    where TValue : struct;