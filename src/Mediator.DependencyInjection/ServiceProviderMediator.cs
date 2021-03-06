using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zapto.Mediator.Wrappers;

namespace Zapto.Mediator;

public class ServiceProviderMediator : IMediator
{
    private readonly IServiceProvider _provider;

    public ServiceProviderMediator(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return RequestWrapper.Get<TResponse>(request.GetType()).Handle(request, cancellationToken, this);
    }

    public ValueTask<TResponse> Send<TResponse>(MediatorNamespace ns, IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return RequestWrapper.Get<TResponse>(request.GetType()).Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        return RequestWrapper.Get(request.GetType()).Handle(request, cancellationToken, this);
    }

    public ValueTask<object?> Send(MediatorNamespace ns, object request, CancellationToken cancellationToken = default)
    {
        return RequestWrapper.Get(request.GetType()).Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
    {
        return _provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>().Handle(_provider, request, cancellationToken);
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

        return services.GetHandler(_provider).Handle(_provider, request, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(MediatorNamespace ns, IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get<TResponse>(request.GetType()).Handle(ns, request, cancellationToken, this);
    }

    public IAsyncEnumerable<object?> CreateStream(MediatorNamespace ns, object request, CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get(request.GetType()).Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get<TResponse>(request.GetType()).Handle(request, cancellationToken, this);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get(request.GetType()).Handle(request, cancellationToken, this);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>
    {
        return _provider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>().Handle(_provider, request, cancellationToken);
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

        return services.GetHandler(_provider).Handle(_provider, request, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
        return NotificationWrapper.Get(notification.GetType()).Handle(notification, cancellationToken, this);
    }

    public ValueTask Publish(MediatorNamespace ns, object notification, CancellationToken cancellationToken = default)
    {
        return NotificationWrapper.Get(notification.GetType()).Handle(ns, notification, cancellationToken, this);
    }

    /// <inheritdoc />
    public async ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        foreach (var handler in _provider.GetServices<INotificationHandler<TNotification>>())
        {
            await handler.Handle(_provider, notification, cancellationToken);
        }

        await _provider
            .GetRequiredService<GenericNotificationHandler<TNotification>>()
            .Handle(_provider, notification, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask Publish<TNotification>(MediatorNamespace ns, TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var services = _provider
            .GetServices<INamespaceNotificationHandler<TNotification>>()
            .Where(i => i.Namespace == ns);

        foreach (var factory in services)
        {
            await factory.GetHandler(_provider).Handle(_provider, notification, cancellationToken);
        }
    }
}
