using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal class NamespaceRequestHandlerProvider<TRequest, TResponse, THandler> : INamespaceRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where THandler : class, IRequestHandler<TRequest, TResponse>
{
    public NamespaceRequestHandlerProvider(MediatorNamespace ns)
    {
        Namespace = ns;
    }

    public MediatorNamespace Namespace { get; }

    public IRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider provider)
        => provider.GetRequiredService<THandler>();
}
