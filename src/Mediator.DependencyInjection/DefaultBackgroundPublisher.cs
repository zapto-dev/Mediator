using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal class DefaultBackgroundPublisher : IBackgroundPublisher
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DefaultBackgroundPublisher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public void Publish(object notification)
    {
        ObserveTask(Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(notification, CancellationToken.None);
        }, CancellationToken.None));
    }

    /// <inheritdoc />
    public void Publish(MediatorNamespace ns, object notification)
    {
        ObserveTask(Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(ns, notification, CancellationToken.None);
        }, CancellationToken.None));
    }

    /// <inheritdoc />
    public void Publish<TNotification>(TNotification notification) where TNotification : INotification
    {
        ObserveTask(Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(notification, CancellationToken.None);
        }, CancellationToken.None));
    }

    /// <inheritdoc />
    public void Publish<TNotification>(MediatorNamespace ns, TNotification notification)
        where TNotification : INotification
    {
        ObserveTask(Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(ns, notification, CancellationToken.None);
        }, CancellationToken.None));
    }

    /// <summary>
    /// Observes faulted fire-and-forget tasks so they do not trigger
    /// <see cref="TaskScheduler.UnobservedTaskException"/> and crash the process.
    /// </summary>
    private static void ObserveTask(Task task)
    {
        task.ContinueWith(
            static t => GC.KeepAlive(t.Exception),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }
}
