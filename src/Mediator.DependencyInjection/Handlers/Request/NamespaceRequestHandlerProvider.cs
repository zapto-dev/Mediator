using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal class NamespaceRequestHandlerProvider<TRequest, TResponse, THandler> : INamespaceRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where THandler : IRequestHandler<TRequest, TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public NamespaceRequestHandlerProvider(MediatorNamespace ns, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Namespace = ns;
    }

    public MediatorNamespace Namespace { get; }

    public THandler Handler => _serviceProvider.GetRequiredService<THandler>();

    IRequestHandler<TRequest, TResponse> INamespaceRequestHandler<TRequest, TResponse>.Handler => Handler;
}
