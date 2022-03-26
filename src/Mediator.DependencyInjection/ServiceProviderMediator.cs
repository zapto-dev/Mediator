using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public class ServiceProviderMediator : IMediator
{
    private readonly IServiceProvider _provider;

    public ServiceProviderMediator(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
    {
        return _provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>().Handle(request, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TRequest, TResponse>(MediatorNamespace ns, TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        var services = _provider
            .GetServices<INamespaceRequestHandler<TRequest, TResponse>>()
            .FirstOrDefault(i => i.Namespace == ns);

        if (services == null)
        {
            throw new InvalidOperationException();
        }

        return services.Handler.Handle(request, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>
    {
        return _provider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>().Handle(request, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(MediatorNamespace ns, TRequest request,
        CancellationToken cancellationToken = default) where TRequest : IStreamRequest<TResponse>
    {
        var services = _provider
            .GetServices<INamespaceStreamRequestHandler<TRequest, TResponse>>()
            .FirstOrDefault(i => i.Namespace == ns);

        if (services == null)
        {
            throw new InvalidOperationException();
        }

        return services.Handler.Handle(request, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        foreach (var handler in _provider.GetServices<INotificationHandler<TNotification>>())
        {
            await handler.Handle(notification, cancellationToken);
        }

        await _provider
            .GetRequiredService<GenericNotificationHandler<TNotification>>()
            .Handle(notification, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask Publish<TNotification>(MediatorNamespace ns, TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var services = _provider
            .GetServices<INamespaceNotificationHandler<TNotification>>()
            .FirstOrDefault(i => i.Namespace == ns);

        if (services == null)
        {
            throw new InvalidOperationException();
        }

        return services.Handler.Handle(notification, cancellationToken);
    }
}
