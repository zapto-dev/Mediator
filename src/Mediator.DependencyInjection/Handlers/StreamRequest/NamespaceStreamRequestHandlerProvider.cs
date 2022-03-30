using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal class NamespaceStreamRequestHandlerProvider<TRequest, TResponse, THandler> : INamespaceStreamRequestHandler<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where THandler : class, IStreamRequestHandler<TRequest, TResponse>
{

    public NamespaceStreamRequestHandlerProvider(MediatorNamespace ns)
    {
        Namespace = ns;
    }

    public MediatorNamespace Namespace { get; }

    public IStreamRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider provider)
        => provider.GetRequiredService<THandler>();
}
