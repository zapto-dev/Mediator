using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator.Wrappers;

internal interface INotificationWrapper
{
    ValueTask Handle(object notification, CancellationToken cancellationToken, IPublisher mediator);

    ValueTask Handle(MediatorNamespace ns, object notification, CancellationToken cancellationToken, IPublisher mediator);
}

internal static class NotificationWrapper
{
    private static readonly ConcurrentDictionary<Type, INotificationWrapper> NotificationHandlers = new();

    public static INotificationWrapper Get(Type type)
    {
        return NotificationHandlers.GetOrAdd(type, static t => (INotificationWrapper)Activator.CreateInstance(typeof(NotificationWrapper<>).MakeGenericType(t)));
    }
}

internal sealed class NotificationWrapper<TNotification> : INotificationWrapper
    where TNotification : INotification
{
    public ValueTask Handle(object notification, CancellationToken cancellationToken, IPublisher mediator)
    {
        return mediator.Publish((TNotification)notification, cancellationToken);
    }

    public ValueTask Handle(MediatorNamespace ns, object notification, CancellationToken cancellationToken, IPublisher mediator)
    {
        return mediator.Publish(ns, (TNotification)notification, cancellationToken);
    }
}
