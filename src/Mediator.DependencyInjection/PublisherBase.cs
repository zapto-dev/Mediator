using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zapto.Mediator.Wrappers;

namespace Zapto.Mediator;

public class PublisherBase : IPublisherBase
{
    private readonly IServiceProvider _provider;

    public PublisherBase(IServiceProvider provider)
    {
        _provider = provider;
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
        var didHandle = false;

        foreach (var handler in _provider.GetServices<INotificationHandler<TNotification>>())
        {
            await handler.Handle(_provider, notification, cancellationToken);
            didHandle = true;
        }

        var didGenericHandle = await _provider
            .GetRequiredService<GenericNotificationHandler<TNotification>>()
            .Handle(_provider, notification, cancellationToken);

        if (!didHandle && !didGenericHandle && _provider.GetService<IDefaultNotificationHandler>() is { } defaultHandler)
        {
            await defaultHandler.Handle(_provider, notification, cancellationToken);
        }
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

    /// <inheritdoc />
    public IDisposable RegisterNotificationHandler(object handler, Func<Func<Task>, Task>? invokeAsync = null, bool queue = true)
    {
        if (queue)
        {
            if (invokeAsync is { } invoker)
            {
                invokeAsync = cb =>
                {
                    _ = Task.Run(() => invoker(cb));
                    return Task.CompletedTask;
                };
            }
            else
            {
                invokeAsync = Task.Run;
            }
        }

        return (IDisposable) typeof(NotificationAttributeHandler<>).MakeGenericType(handler.GetType())
            .GetMethod(nameof(NotificationAttributeHandler<object>.RegisterHandlers), BindingFlags.Static | BindingFlags.Public)!
            .Invoke(null, new[] { _provider, handler, invokeAsync });
    }
}
