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
    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(notification, CancellationToken.None);
        }, CancellationToken.None);

        return default;
    }

    /// <inheritdoc />
    public ValueTask Publish(MediatorNamespace ns, object notification, CancellationToken cancellationToken = default)
    {
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(ns, notification, CancellationToken.None);
        }, CancellationToken.None);

        return default;
    }

    /// <inheritdoc />
    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(notification, CancellationToken.None);
        }, CancellationToken.None);

        return default;
    }

    /// <inheritdoc />
    public ValueTask Publish<TNotification>(MediatorNamespace ns, TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(ns, notification, CancellationToken.None);
        }, CancellationToken.None);

        return default;
    }
}
