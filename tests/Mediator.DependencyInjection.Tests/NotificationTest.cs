using System;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Mediator.DependencyInjection.Tests;

public record Notification : INotification;

public class NotificationTest
{
    [Fact]
    public async Task TestNotification()
    {
        var handler = new Mock<INotificationHandler<Notification>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(handler.Object);
                b.AddNotificationHandler(handler.Object);
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Notification());

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task TestNotificationObject()
    {
        var handler = new Mock<INotificationHandler<Notification>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(handler.Object);
                b.AddNotificationHandler(handler.Object);
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish((object) new Notification());

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task TestNamespaceNotification()
    {
        var ns = new MediatorNamespace("test");
        var handler = new Mock<INotificationHandler<Notification>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(handler.Object);
                b.AddNotificationHandler(handler.Object);

                b.AddNamespace(ns)
                    .AddNotificationHandler(handler.Object)
                    .AddNotificationHandler(handler.Object);
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Notification());
        await mediator.Publish(ns, new Notification());

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
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
}
