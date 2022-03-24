using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal class NamespaceStreamRequestHandlerProvider<TRequest, TResponse, THandler> : INamespaceStreamRequestHandler<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where THandler : IStreamRequestHandler<TRequest, TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public NamespaceStreamRequestHandlerProvider(MediatorNamespace ns, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Namespace = ns;
    }

    public MediatorNamespace Namespace { get; }

    public THandler Handler => _serviceProvider.GetRequiredService<THandler>();

    IStreamRequestHandler<TRequest, TResponse> INamespaceStreamRequestHandler<TRequest, TResponse>.Handler => Handler;
}
