using System;
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

    /// <summary>
    /// Set to <c>true</c> under <see cref="INotificationCache.Lock"/> when the
    /// handler is disposed. Checked (without lock) as a best-effort guard in
    /// <see cref="InvokeAsync"/> to skip already-disposed handlers.
    /// </summary>
    bool IsDisposed { get; set; }

    ValueTask InvokeAsync(IServiceProvider provider, object notification, CancellationToken cancellationToken);
}

internal interface INotificationCache
{
    List<IHandlerRegistration> Registrations { get; }

    SemaphoreSlim Lock { get; }
}

internal sealed class GenericNotificationCache<TNotification> : INotificationCache
{
    public GenericNotificationCache(IEnumerable<GenericNotificationRegistration> registrations)
    {
        var notificationType = typeof(TNotification);
        if (notificationType.IsGenericType)
        {
            var genericType = notificationType.GetGenericTypeDefinition();
            MatchingRegistrations = GenericTypeHelper.CacheMatchingRegistrations(
                registrations,
                r => r.NotificationType,
                genericType);
        }
        else
        {
            MatchingRegistrations = new List<GenericNotificationRegistration>();
        }
    }

    private List<Type>? _handlerTypes;
    public List<Type>? HandlerTypes { get => Volatile.Read(ref _handlerTypes); set => Volatile.Write(ref _handlerTypes, value); }

    public List<GenericNotificationRegistration> MatchingRegistrations { get; }

    public List<IHandlerRegistration> Registrations { get; } = new();

    public SemaphoreSlim Lock { get; } = new(1, 1);
}

internal sealed class GenericNotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly GenericNotificationCache<TNotification> _cache;
    private readonly IServiceProvider _serviceProvider;

    public GenericNotificationHandler(IServiceProvider serviceProvider, GenericNotificationCache<TNotification> cache)
    {
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
