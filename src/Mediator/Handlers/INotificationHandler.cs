using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

public interface INotificationHandler
{
}

/// <summary>
/// Defines a handler for a notification
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled</typeparam>
public interface INotificationHandler<in TNotification> : INotificationHandler
    where TNotification : INotification
{
    /// <summary>
    /// Handles a notification
    /// </summary>
    /// <param name="notification">The notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask Handle(TNotification notification, CancellationToken cancellationToken);
}

/// <summary>
/// Wrapper class for a synchronous notification handler
/// </summary>
/// <typeparam name="TNotification">The notification type</typeparam>
public abstract class NotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    ValueTask INotificationHandler<TNotification>.Handle(TNotification notification, CancellationToken cancellationToken)
    {
        Handle(notification);
        return default;
    }

    /// <summary>
    /// Override in a derived class for the handler logic
    /// </summary>
    /// <param name="notification">Notification</param>
    protected abstract void Handle(TNotification notification);
}
