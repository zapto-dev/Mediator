using MediatR;

namespace Zapto.Mediator;

internal class NamespaceStreamRequestHandler<TRequest, TResponse> : INamespaceStreamRequestHandler<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public NamespaceStreamRequestHandler(MediatorNamespace ns, IStreamRequestHandler<TRequest, TResponse> handler)
    {
        Namespace = ns;
        Handler = handler;
    }

    public MediatorNamespace Namespace { get; }

    public IStreamRequestHandler<TRequest, TResponse> Handler { get; }
}
