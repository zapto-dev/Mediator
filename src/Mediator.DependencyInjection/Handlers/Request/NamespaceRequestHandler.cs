using MediatR;

namespace Zapto.Mediator;

internal class NamespaceRequestHandler<TRequest, TResponse> : INamespaceRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public NamespaceRequestHandler(MediatorNamespace ns, IRequestHandler<TRequest, TResponse> handler)
    {
        Namespace = ns;
        Handler = handler;
    }

    public MediatorNamespace Namespace { get; }

    public IRequestHandler<TRequest, TResponse> Handler { get; }
}
