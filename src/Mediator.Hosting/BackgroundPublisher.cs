using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zapto.Mediator.Options;
using Zapto.Mediator.Services;

namespace Zapto.Mediator;

internal class BackgroundPublisher : IBackgroundPublisher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BackgroundQueueService _backgroundQueueService;
    private readonly IHostApplicationLifetime? _applicationLifetime;
    private readonly IOptions<MediatorBackgroundOptions> _options;

    public BackgroundPublisher(
        BackgroundQueueService backgroundQueueService,
        IServiceScopeFactory scopeFactory,
        IOptions<MediatorBackgroundOptions>? options = null,
        IHostApplicationLifetime? applicationLifetime = null)
    {
        _backgroundQueueService = backgroundQueueService;
        _scopeFactory = scopeFactory;
        _options = options ?? new OptionsWrapper<MediatorBackgroundOptions>(new MediatorBackgroundOptions());
        _applicationLifetime = applicationLifetime;
    }

    public CancellationToken CancellationToken => _options.Value.CancelWorkerItemsWhenStopping && _applicationLifetime is not null
        ? _applicationLifetime.ApplicationStopping
        : CancellationToken.None;

    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
        _backgroundQueueService.QueueBackgroundWorkItem(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(notification, CancellationToken);
        }, notification);

        return default;
    }

    public ValueTask Publish(MediatorNamespace ns, object notification, CancellationToken cancellationToken = default)
    {
        _backgroundQueueService.QueueBackgroundWorkItem(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(ns, notification, CancellationToken);
        }, notification);

        return default;
    }

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        _backgroundQueueService.QueueBackgroundWorkItem(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(notification, CancellationToken);
        }, notification);

        return default;
    }

    public ValueTask Publish<TNotification>(MediatorNamespace ns, TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        _backgroundQueueService.QueueBackgroundWorkItem(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();

            await mediator.Publish(ns, notification, CancellationToken);
        }, notification);

        return default;
    }
}
