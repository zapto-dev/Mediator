using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Zapto.Mediator.Services;

internal class BackgroundQueueHostedService : IHostedService
{
    private readonly BackgroundQueueService _backgroundQueueService;

    public BackgroundQueueHostedService(BackgroundQueueService backgroundQueueService)
    {
        _backgroundQueueService = backgroundQueueService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _backgroundQueueService.WaitForBackgroundTasksAsync(cancellationToken);
    }
}
