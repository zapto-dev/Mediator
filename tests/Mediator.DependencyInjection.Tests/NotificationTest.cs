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

        // Before registration – handler must not be called
        await mediator.Publish((object) new Notification());
        Assert.Equal(0, handler.Count);

        // Register and publish – handler is invoked
        var disposable = mediator.RegisterNotificationHandler(handler);
        await mediator.Publish((object) new Notification());
        Assert.Equal(1, handler.Count);

        // Dispose and publish again – handler must not be called a second time
        disposable.Dispose();
        await mediator.Publish((object) new Notification());
        Assert.Equal(1, handler.Count);
    }

    [Theory]
    [InlineData(typeof(ValueTaskNotificationHandler))]
    [InlineData(typeof(TaskNotificationHandler))]
    [InlineData(typeof(VoidNotificationHandler))]
    public async Task TestTemporaryHandlerDisposedBeforeInvocationIsIgnored(Type type)
    {
        // Verifies the IsDisposed guard: when a handler with a deferred invoker is disposed
        // before the callback fires, InvokeAsync should be a no-op.
        var tcs = new TaskCompletionSource<bool>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(_ => { })
            .BuildServiceProvider();

        var handler = (ITestNotificationHandler) Activator.CreateInstance(type)!;
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Register with a deferred invoker so the IsDisposed path in HandlerRegistration.InvokeAsync is exercised
        var disposable = mediator.RegisterNotificationHandler(handler, cb => tcs.Task.ContinueWith(_ => cb()), queue: false);

        // Start publish without awaiting — the middleware blocks on tcs.Task
        var publishTask = mediator.Publish((object) new Notification()).AsTask();

        // Dispose before the deferred callback fires
        disposable.Dispose();

        // Trigger the pending callback now – IsDisposed is true so the handler must not be called
        tcs.SetResult(true);

        // Await the publish (now unblocked) and give continuations time to settle
        await publishTask;
        await Task.Delay(50);

        Assert.Equal(0, handler.Count);
    }

    [Fact]
    public async Task QueuedHandlerDoesNotBlockPublishEvenWhenNeverCompleting()
    {
        // Simulates the Blazor shutdown scenario: a handler registered with queue: true
        // that never completes must not block Publish or prevent disposal.
        var tcs = new TaskCompletionSource<bool>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(_ => { })
            .BuildServiceProvider();

        var handler = new ValueTaskNotificationHandler();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // invoker that returns a never-completing task (simulating Blazor's InvokeAsync with a stuck handler)
        var disposable = mediator.RegisterNotificationHandler(
            handler,
            _ => tcs.Task, // blocks until tcs is completed
            queue: true);

        // Publish must return immediately because queue: true wraps the invoker as fire-and-forget
        var publishTask = mediator.Publish((object) new Notification());
        var completed = publishTask.IsCompleted || await Task.WhenAny(publishTask.AsTask(), Task.Delay(1000)) == publishTask.AsTask();
        Assert.True(completed, "Publish should return immediately with queue: true, even when the handler never completes");

        // Disposal must succeed without blocking
        disposable.Dispose();

        // Complete the stuck task so it doesn't prevent the test process from exiting
        tcs.SetCanceled();
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
        publisher.Background.Publish(new Notification());

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
