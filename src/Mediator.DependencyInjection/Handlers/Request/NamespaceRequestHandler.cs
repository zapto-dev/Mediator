using System;
using MediatR;

namespace Zapto.Mediator;

internal class NamespaceRequestHandler<TRequest, TResponse> : INamespaceRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IRequestHandler<TRequest, TResponse> _handler;

    public NamespaceRequestHandler(MediatorNamespace ns, IRequestHandler<TRequest, TResponse> handler)
    {
        Namespace = ns;
        _handler = handler;
    }

    public MediatorNamespace Namespace { get; }

    public IRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider provider)
    {
        return _handler;
    }
}
