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

internal sealed class GenericNotificationCache<TNotification>
{
    public Type? NotificationType { get; set; }
}

internal sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly GenericNotificationCache<TNotification> _cache;
    private readonly IEnumerable<GenericNotificationRegistration> _enumerable;
    private readonly IServiceProvider _serviceProvider;

    public GenericNotificationHandler(IEnumerable<GenericNotificationRegistration> enumerable, IServiceProvider serviceProvider, GenericNotificationCache<TNotification> cache)
    {
        _enumerable = enumerable;
        _serviceProvider = serviceProvider;
        _cache = cache;
    }

    public async ValueTask Handle(IServiceProvider provider, TNotification notification, CancellationToken ct)
    {
        if (_cache.NotificationType is {} cachedType)
        {
            foreach (var handler in _serviceProvider.GetServices(cachedType))
            {
                await ((INotificationHandler<TNotification>)handler!).Handle(provider, notification, ct);
            }

            return;
        }

        var notificationType = typeof(TNotification);

        if (!notificationType.IsGenericType)
        {
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

            _cache.NotificationType = type;

            foreach (var handler in _serviceProvider.GetServices(type))
            {
                await ((INotificationHandler<TNotification>)handler!).Handle(provider, notification, ct);
            }

            break;
        }
    }
}
