using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public record struct GenericVoidRequest<T>(T Value) : IRequest;

public record GenericVoidRequestHandler<T> : IRequestHandler<GenericVoidRequest<T>>
{
    public int CallCount { get; private set; }

    public ValueTask Handle(IServiceProvider provider, GenericVoidRequest<T> request, CancellationToken cancellationToken)
    {
        CallCount++;
        return new ValueTask();
    }
}
