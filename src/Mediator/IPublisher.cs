using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

public interface IPublisher
{
    /// <summary>
    /// Gets the publisher that runs in the background.
    /// </summary>
    IBackgroundPublisher Background { get; }

    /// <summary>
    /// Register a temporary notification handler
    /// </summary>
    /// <param name="handler">The handler to register</param>
    /// <param name="invokeAsync">Middleware to invoke the handler</param>
    /// <param name="queue">Queue the handler instead of invoking it immediately</param>
    /// <returns>A disposable object that can be used to unregister the handler.</returns>
    IDisposable RegisterNotificationHandler(object handler, Func<Func<Task>, Task>? invokeAsync = null, bool queue = true);

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
}

public interface IBackgroundPublisher
{
    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <returns>A task that represents the publish operation.</returns>
    void Publish(object notification);

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="notification">Notification object</param>
    /// <returns>A task that represents the publish operation.</returns>
    void Publish(MediatorNamespace ns, object notification);

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <returns>A task that represents the publish operation.</returns>
    void Publish<TNotification>(TNotification notification)
        where TNotification : INotification;

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="notification">Notification object</param>
    /// <returns>A task that represents the publish operation.</returns>
    void Publish<TNotification>(MediatorNamespace ns, TNotification notification)
        where TNotification : INotification;
}
