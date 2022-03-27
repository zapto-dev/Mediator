using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal sealed class GenericNotificationRegistration
{
    public GenericNotificationRegistration(Type notificationType, Type handlerType)
    {
        NotificationType = notificationType;
        HandlerType = handlerType;
    }

    public Type NotificationType { get; }

    public Type HandlerType { get; }
}

internal sealed class GenericNotificationCache
{
    public ConcurrentDictionary<Type, Type> Handlers { get; } = new();
}

internal sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly GenericNotificationCache _cache;
    private readonly IEnumerable<GenericNotificationRegistration> _enumerable;
    private readonly IServiceProvider _serviceProvider;

    public GenericNotificationHandler(IEnumerable<GenericNotificationRegistration> enumerable, IServiceProvider serviceProvider, GenericNotificationCache cache)
    {
        _enumerable = enumerable;
        _serviceProvider = serviceProvider;
        _cache = cache;
    }

    public async ValueTask Handle(TNotification notification, CancellationToken ct)
    {
        var notificationType = typeof(TNotification);

        if (!notificationType.IsGenericType)
        {
            return;
        }

        if (_cache.Handlers.TryGetValue(notificationType, out var handlerType))
        {
            foreach (var handler in _serviceProvider.GetServices(handlerType))
            {
                await ((INotificationHandler<TNotification>)handler!).Handle(notification, ct);
            }

            return;
        }

        var arguments = notificationType.GetGenericArguments();
        var genericType = notificationType.GetGenericTypeDefinition();

        foreach (var registration in _enumerable)
        {
            if (registration.NotificationType != genericType)
            {
                continue;
            }

            var type = registration.HandlerType.MakeGenericType(arguments);

            _cache.Handlers.TryAdd(notificationType, type);

            foreach (var handler in _serviceProvider.GetServices(type))
            {
                await ((INotificationHandler<TNotification>)handler!).Handle(notification, ct);
            }

            break;
        }
    }
}
