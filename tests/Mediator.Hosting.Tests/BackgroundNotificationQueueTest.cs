using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;
using Zapto.Mediator;
using Zapto.Mediator.Services;

namespace Mediator.Hosting.Tests;

public record Notification : INotification;

public class BackgroundNotificationQueueTest
{
    [Fact]
    public async Task NotificationGettingCalled()
    {
        using var cts = new CancellationTokenSource();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var handler = Substitute.For<INotificationHandler<Notification>>();

        var host = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        host.Services.AddMediator(b =>
        {
            b.AddNotificationHandler(handler);
            b.AddHostingBackgroundScheduler();
        });

        var app = host.Build();

        await app.StartAsync();

        var backgroundQueue = app.Services.GetRequiredService<BackgroundQueueService>();
        var mediator = app.Services.GetRequiredService<IBackgroundPublisher>();

        mediator.Publish(new Notification());

        await backgroundQueue.WaitForBackgroundTasksAsync(cts.Token);

        _ = handler.Received(1).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());

        await app.StopAsync();
    }

    [Fact]
    public async Task MaxOneNotificationGettingCalled()
    {
        using var cts = new CancellationTokenSource();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var tcs = new TaskCompletionSource<bool>();
        var handler = Substitute.For<INotificationHandler<Notification>>();

        var host = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        host.Services.AddMediator(b =>
        {
            b.AddNotificationHandler(async (Notification _) => await tcs.Task);
            b.AddNotificationHandler(handler);
            b.AddHostingBackgroundScheduler(options =>
            {
                options.MaxDegreeOfParallelism = 1;
            });
        });

        var app = host.Build();

        await app.StartAsync();

        var backgroundQueue = app.Services.GetRequiredService<BackgroundQueueService>();
        var mediator = app.Services.GetRequiredService<IBackgroundPublisher>();

        var notification1 = new Notification();
        var notification2 = new Notification();
        mediator.Publish(notification1);
        mediator.Publish(notification2);

        // Handler should not be called yet
        _ = handler.Received(0).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());

        var currentNotifications = backgroundQueue.GetRunningNotifications();

        Assert.Single(currentNotifications);
        Assert.Equal(notification1, currentNotifications[0]);

        // Continue
        tcs.SetResult(true);

        await backgroundQueue.WaitForBackgroundTasksAsync(cts.Token);
        _ = handler.Received(2).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());

        await app.StopAsync();
    }

    [Fact]
    public async Task MaxTwoNotificationGettingCalled()
    {
        using var cts = new CancellationTokenSource();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var tcs = new TaskCompletionSource<bool>();
        var handler = Substitute.For<INotificationHandler<Notification>>();

        var host = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        host.Services.AddMediator(b =>
        {
            b.AddNotificationHandler(async (Notification _) => await tcs.Task);
            b.AddNotificationHandler(handler);
            b.AddHostingBackgroundScheduler(options =>
            {
                options.MaxDegreeOfParallelism = 2;
            });
        });

        var app = host.Build();

        await app.StartAsync();

        var backgroundQueue = app.Services.GetRequiredService<BackgroundQueueService>();
        var mediator = app.Services.GetRequiredService<IBackgroundPublisher>();

        var notification1 = new Notification();
        var notification2 = new Notification();
        mediator.Publish(notification1);
        mediator.Publish(notification2);

        // Handler should not be called yet
        _ = handler.Received(0).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());

        var currentNotifications = backgroundQueue.GetRunningNotifications();

        Assert.Equal(2, currentNotifications.Length);
        Assert.Equal(notification1, currentNotifications[0]);
        Assert.Equal(notification2, currentNotifications[1]);

        // Continue
        tcs.SetResult(true);

        await backgroundQueue.WaitForBackgroundTasksAsync(cts.Token);
        _ = handler.Received(2).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());

        await app.StopAsync();
    }

    [Fact]
    public async Task DisallowBackgroundWorkWhileStopping()
    {
        var handler = Substitute.For<INotificationHandler<Notification>>();

        var host = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        host.Services.AddMediator(b =>
        {
            b.AddNotificationHandler(handler);
            b.AddHostingBackgroundScheduler(options =>
            {
                options.AllowBackgroundWorkWhileStopping = false;
            });
        });

        var app = host.Build();

        await app.StartAsync();

        var mediator = app.Services.GetRequiredService<IBackgroundPublisher>();

        await app.StopAsync();

        Assert.Throws<OperationCanceledException>(() => mediator.Publish(new Notification()));
    }

    [Fact]
    public async Task GracefulShutdown()
    {
        using var cts = new CancellationTokenSource();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var tcs = new TaskCompletionSource<bool>();
        var handler = Substitute.For<INotificationHandler<Notification>>();

        var host = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        host.Services.AddMediator(b =>
        {
            b.AddNotificationHandler(async (Notification _) => await tcs.Task);
            b.AddNotificationHandler(handler);
            b.AddHostingBackgroundScheduler(options =>
            {
                options.MaxDegreeOfParallelism = 1;
            });
        });

        var app = host.Build();

        await app.StartAsync();

        var mediator = app.Services.GetRequiredService<IBackgroundPublisher>();

        mediator.Publish(new Notification());

        var stopTask = app.StopAsync(cts.Token);

        // Handler should not be called yet
        Assert.False(stopTask.IsCompleted);
        _ = handler.Received(0).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());

        tcs.SetResult(true);
        await stopTask;

        // Handler should be called
        _ = handler.Received(1).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StopNotification()
    {
        var cancelled = new StrongBox<bool>();

        var host = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        host.Services.AddMediator(b =>
        {
            b.AddNotificationHandler(void (Notification _, CancellationToken token) => cancelled.Value = token.IsCancellationRequested);
            b.AddHostingBackgroundScheduler(options =>
            {
                options.CancelWorkerItemsWhenStopping = true;
            });
        });

        var app = host.Build();

        await app.StartAsync();

        var mediator = app.Services.GetRequiredService<IBackgroundPublisher>();

        mediator.Publish(new Notification());
        Assert.False(cancelled.Value);

        await app.StopAsync();

        mediator.Publish(new Notification());
        Assert.True(cancelled.Value);
    }
}
