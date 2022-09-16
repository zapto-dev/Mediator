using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

public interface IPublisher
{
    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    ValueTask Publish(object notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    ValueTask Publish(MediatorNamespace ns, object notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    ValueTask Publish<TNotification>(MediatorNamespace ns, TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Register a temporary notification handler
    /// </summary>
    /// <param name="handler">The handler to register</param>
    /// <param name="invokeAsync">Middleware to invoke the handler</param>
    /// <param name="queue">Queue the handler instead of invoking it immediately</param>
    /// <returns>A disposable object that can be used to unregister the handler.</returns>
    IDisposable RegisterNotificationHandler(object handler, Func<Func<Task>, Task>? invokeAsync = null, bool queue = false);
}
