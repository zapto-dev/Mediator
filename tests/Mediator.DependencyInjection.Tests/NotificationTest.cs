using System;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mediator.DependencyInjection.Tests;

public record Notification : INotification;

public class NotificationTest
{
    [Fact]
    public async Task TestNotification()
    {
        var invokedOne = false;
        var invokedTwo = false;

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddNotificationHandler((Notification _) =>
            {
                invokedOne = true;
            })
            .AddNotificationHandler((Notification _) =>
            {
                invokedTwo = true;
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Notification());

        Assert.True(invokedOne, "Notification handler one was not invoked");
        Assert.True(invokedTwo, "Notification handler two was not invoked");
    }


    [Fact]
    public async Task TestNamespaceNotification()
    {
        var ns = new MediatorNamespace("test");
        const int expectedGlobal = 1;
        const int expectedNs = 2;

        var result = 0;

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddNotificationHandler((Notification _) =>
            {
                result = expectedGlobal;
            })
            .AddNotificationHandler((Notification _) =>
            {
                result = expectedNs;
            }, ns)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Notification());
        Assert.Equal(expectedGlobal, result);

        await mediator.Publish(ns, new Notification());
        Assert.Equal(expectedNs, result);
    }
}
