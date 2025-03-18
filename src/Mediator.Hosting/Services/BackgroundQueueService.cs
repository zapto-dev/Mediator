using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zapto.Mediator.Options;

namespace Zapto.Mediator.Services;

public class BackgroundQueueService
{
    private readonly IOptions<MediatorBackgroundOptions> _options;
    private readonly ConcurrentQueue<Func<Task>> _workItems = new();
    private readonly List<Worker> _workers = new();
    private readonly ILogger<BackgroundQueueService>? _logger;
    private readonly IHostApplicationLifetime? _applicationLifetime;

    public BackgroundQueueService(IHostApplicationLifetime? applicationLifetime = null, ILogger<BackgroundQueueService>? logger = null, IOptions<MediatorBackgroundOptions>? options = null)
    {
        _options = options ?? new OptionsWrapper<MediatorBackgroundOptions>(new MediatorBackgroundOptions());
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    public void QueueBackgroundWorkItem(Func<Task> workItem, object notification)
    {
        if (_applicationLifetime is { ApplicationStopping.IsCancellationRequested: true } &&
            !_options.Value.AllowBackgroundWorkWhileStopping)
        {
            throw new OperationCanceledException("Cannot schedule work item since the application is stopping");
        }

        _workItems.Enqueue(workItem);

        lock (_workers)
        {
            if (_workers.Count < _options.Value.MaxDegreeOfParallelism)
            {
                var worker = new Worker
                {
                    Notification = notification
                };

                worker.Task = Task.Factory.StartNew(() => ProcessBackgroundWorkItem(worker), TaskCreationOptions.LongRunning);
                _workers.Add(worker);
            }
        }
    }

    public object[] GetRunningNotifications()
    {
        lock (_workers)
        {
            return _workers.Select(w => w.Notification).ToArray();
        }
    }

    public async Task WaitForBackgroundTasksAsync(CancellationToken cancellationToken)
    {
#if !NET
        var tcs = new TaskCompletionSource<bool>();
        
        using var registration = cancellationToken.Register(() => tcs.TrySetResult(true));

        Task? resultingTask = null;
#endif

        while (true)
        {
            Task task;

            lock (_workers)
            {
                if (_workers.Count == 0)
                {
                    break;
                }

                task = _workers[0].Task;
            }

            try
            {
#if NET
                await task.WaitAsync(cancellationToken);
#else
                resultingTask = await Task.WhenAny(task, tcs.Task);
#endif
            }
            catch
            {
                // ignore
            }

#if !NET
            if (resultingTask == tcs.Task)
            {
                throw new OperationCanceledException();
            }
#endif
        }
    }

    private async Task ProcessBackgroundWorkItem(Worker worker)
    {
        while (true)
        {
            // Check if there are no more work items and remove worker
            if (_workItems.IsEmpty)
            {
                lock (_workers)
                {
                    if (_workItems.IsEmpty)
                    {
                        _workers.Remove(worker);
                        break;
                    }
                }
            }

            // Process next work item
            if (!_workItems.TryDequeue(out var workItem))
            {
                continue;
            }

            try
            {
                await workItem();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred executing background notification");
            }
        }
    }

    private class Worker
    {
        public Task Task { get; set; } = Task.CompletedTask;

        public object Notification { get; set; } = null!;
    }
}
