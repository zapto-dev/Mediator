using System;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Mediator.DependencyInjection.Tests;

public record Notification : INotification;

public class NotificationTest
{
    [Fact]
    public async Task TestNotification()
    {
        var handler = Substitute.For<INotificationHandler<Notification>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(handler);
                b.AddNotificationHandler(handler);
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Notification());

        _ = handler.Received(2).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestNotificationObject()
    {
        var handler = Substitute.For<INotificationHandler<Notification>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(handler);
                b.AddNotificationHandler(handler);
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish((object) new Notification());

        _ = handler.Received(2).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestNamespaceNotification()
    {
        var ns = new MediatorNamespace("test");
        var handler = Substitute.For<INotificationHandler<Notification>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(handler);
                b.AddNotificationHandler(handler);

                b.AddNamespace(ns)
                    .AddNotificationHandler(handler)
                    .AddNotificationHandler(handler);
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Notification());
        await mediator.Publish(ns, new Notification());

        _ = handler.Received(requiredNumberOfCalls: 4)
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestNoRegistration()
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(_ => { })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Notification());
    }

    [Fact]
    public async Task TestNoRegistrationNotification()
    {
        var ns = new MediatorNamespace("test");

        var serviceProvider = new ServiceCollection()
            .AddMediator(_ => { })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(ns, new Notification());
    }

    [Theory]
    [InlineData(typeof(ValueTaskNotificationHandler))]
    [InlineData(typeof(TaskNotificationHandler))]
    [InlineData(typeof(VoidNotificationHandler))]
    public async Task TestTemporaryHandler(Type type)
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(_ => { })
            .BuildServiceProvider();

        var handler = (ITestNotificationHandler) Activator.CreateInstance(type)!;
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish((object) new Notification());

        Assert.Equal(0, handler.Count);

        var disposable = mediator.RegisterNotificationHandler(handler);
        await mediator.Publish((object) new Notification());

        Assert.Equal(1, handler.Count);

        disposable.Dispose();
        await mediator.Publish((object) new Notification());

        Assert.Equal(1, handler.Count);
    }

    [Fact]
    public async Task BackgroundPublisher()
    {
        var before = Substitute.For<INotificationHandler<Notification>>();
        var after = Substitute.For<INotificationHandler<Notification>>();
        var tcs = new TaskCompletionSource<object?>();

        var tcsBefore = new TaskCompletionSource<object?>();
        var tcsAfter = new TaskCompletionSource<object?>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(before);
                b.AddNotificationHandler((Notification _) => tcsBefore.SetResult(null));
                b.AddNotificationHandler((Notification _) => tcs.Task);
                b.AddNotificationHandler(after);
                b.AddNotificationHandler((Notification _) => tcsAfter.SetResult(null));
            })
            .BuildServiceProvider();

        var publisher = serviceProvider.GetRequiredService<IPublisher>();
        var timeOutTask = Task.Delay(3000);
        var resultTask = await Task.WhenAny(
            publisher.Background.Publish(new Notification()).AsTask(),
            timeOutTask);

        // Ensure that the task completes in the background.
        Assert.NotEqual(timeOutTask, resultTask);

        // Ensure that the before handler is called.
        Assert.Equal(tcsBefore.Task, await Task.WhenAny(tcsBefore.Task, Task.Delay(1000)));

        // Before should be called, but after should not.
        _ = before.Received(1).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        _ = after.DidNotReceive().Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());

        // Continue the task and ensure that after is called.
        tcs.SetResult(null);

        // Ensure that the after handler is called.
        Assert.Equal(tcsAfter.Task, await Task.WhenAny(tcsAfter.Task, Task.Delay(1000)));
        _ = after.Received(1).Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    public interface ITestNotificationHandler
    {
        int Count { get; }
    }

    public class ValueTaskNotificationHandler : ITestNotificationHandler
    {
        public int Count { get; private set; }

        [NotificationHandler]
        public ValueTask Handle(Notification notification)
        {
            Count++;
            return default;
        }
    }

    public class TaskNotificationHandler : ITestNotificationHandler
    {
        public int Count { get; private set; }

        [NotificationHandler]
        public Task Handle(Notification notification)
        {
            Count++;
            return Task.CompletedTask;
        }
    }

    public class VoidNotificationHandler : ITestNotificationHandler
    {
        public int Count { get; private set; }

        [NotificationHandler]
        public void Handle(Notification notification)
        {
            Count++;
        }
    }
}
