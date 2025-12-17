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

internal interface IHandlerRegistration
{
    INotificationCache Owner { get; }

    ValueTask InvokeAsync(IServiceProvider provider, object notification, CancellationToken cancellationToken);
}

internal interface INotificationCache
{
    List<IHandlerRegistration> Registrations { get; }

    SemaphoreSlim Lock { get; }
}

internal sealed class GenericNotificationCache<TNotification> : INotificationCache
{
    public List<Type>? HandlerTypes { get; set; }

    public List<GenericNotificationRegistration>? MatchingRegistrations { get; set; }

    public List<IHandlerRegistration> Registrations { get; } = new();

    public SemaphoreSlim Lock { get; } = new(1, 1);
}

internal sealed class GenericNotificationHandler<TNotification>
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

    public async ValueTask<bool> Handle(IServiceProvider provider, TNotification notification, CancellationToken ct)
    {
        if (_cache.Registrations.Count > 0)
        {
            await _cache.Lock.WaitAsync(ct);
            var registrations = _cache.Registrations.ToArray();
            _cache.Lock.Release();

            foreach (var registration in registrations)
            {
                await registration.InvokeAsync(provider, notification, ct);
            }
        }

        if (_cache.HandlerTypes is {} cachedTypes)
        {
            foreach (var cachedType in cachedTypes)
            {
                var handler = _serviceProvider.GetRequiredService(cachedType);

                await ((INotificationHandler<TNotification>)handler).Handle(provider, notification, ct);
            }

            return cachedTypes.Count > 0;
        }

        var notificationType = typeof(TNotification);

        if (!notificationType.IsGenericType)
        {
            return false;
        }

        var arguments = notificationType.GetGenericArguments();
        var genericType = notificationType.GetGenericTypeDefinition();
        var handlerTypes = new List<Type>();

        // Cache matching registrations on first call to avoid enumerating on every Handle call
        if (_cache.MatchingRegistrations == null)
        {
            _cache.MatchingRegistrations = GenericTypeHelper.CacheMatchingRegistrations(
                _enumerable,
                r => r.NotificationType,
                genericType);
        }

        foreach (var registration in _cache.MatchingRegistrations)
        {
            Type type;
            if (registration.HandlerType.IsGenericType)
            {
                if (!GenericTypeHelper.CanMakeGenericType(registration.HandlerType, arguments))
                {
                    continue;
                }

                type = registration.HandlerType.MakeGenericType(arguments);
            }
            else
            {
                type = registration.HandlerType;
            }

            var handler = _serviceProvider.GetRequiredService(type);

            await ((INotificationHandler<TNotification>)handler!).Handle(provider, notification, ct);

            handlerTypes.Add(type);
        }

        _cache.HandlerTypes = handlerTypes;
        return handlerTypes.Count > 0;
    }
}
