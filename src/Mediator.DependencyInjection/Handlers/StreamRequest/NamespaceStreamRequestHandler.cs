using System;
using MediatR;

namespace Zapto.Mediator;

internal class NamespaceStreamRequestHandler<TRequest, TResponse> : INamespaceStreamRequestHandler<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly IStreamRequestHandler<TRequest, TResponse> _handler;

    public NamespaceStreamRequestHandler(MediatorNamespace ns, IStreamRequestHandler<TRequest, TResponse> handler)
    {
        Namespace = ns;
        _handler = handler;
    }

    public MediatorNamespace Namespace { get; }

    public IStreamRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider provider)
    {
        return _handler;
    }
}
