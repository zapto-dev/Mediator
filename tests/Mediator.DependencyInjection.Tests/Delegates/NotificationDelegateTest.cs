using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Delegates;

public class NotificationDelegateTest
{
    [Fact]
    public async Task Valid()
    {
        var called = false;

        await using var provider = new ServiceCollection()
            .AddMediator()
            .AddNotificationHandler((Notification _) =>
            {
                called = true;
            })
            .BuildServiceProvider();

        await provider.GetRequiredService<IMediator>()
            .Publish(new Notification());

        Assert.True(called);
    }

    [Fact]
    public async Task ValidTask()
    {
        var called = false;

        await using var provider = new ServiceCollection()
            .AddMediator()
            .AddNotificationHandler((Notification _) =>
            {
                called = true;
                return Task.CompletedTask;
            })
            .BuildServiceProvider();

        await provider.GetRequiredService<IMediator>()
            .Publish(new Notification());

        Assert.True(called);
    }

    [Fact]
    public async Task ValidValueTask()
    {
        var called = false;

        await using var provider = new ServiceCollection()
            .AddMediator()
            .AddNotificationHandler((Notification _) =>
            {
                called = true;
                return default(ValueTask);
            })
            .BuildServiceProvider();

        await provider.GetRequiredService<IMediator>()
            .Publish(new Notification());

        Assert.True(called);
    }

    [Fact]
    public async Task ValidIgnoreValue()
    {
        var called = false;

        await using var provider = new ServiceCollection()
            .AddMediator()
            .AddNotificationHandler((Notification _) =>
            {
                called = true;
                return true;
            })
            .BuildServiceProvider();

        await provider.GetRequiredService<IMediator>()
            .Publish(new Notification());

        Assert.True(called);
    }

    [Fact]
    public void NoNotification()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new ServiceCollection()
                .AddMediator()
                .AddNotificationHandler(() => {});
        });
    }

    [Fact]
    public void MultipleNotifications()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            new ServiceCollection()
                .AddMediator()
                .AddNotificationHandler((Notification _, Notification _) => {});
        });
    }
}
