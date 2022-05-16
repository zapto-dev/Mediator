﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class NotificationGenericTest
{
    [Fact]
    public async Task Valid()
    {
        const string expected = "success";
        var result = new Result();

        await using var provider = new ServiceCollection()
            .AddMediator()
            .AddSingleton(result)
            .AddNotificationHandler(typeof(GenericNotificationHandler<>))
            .AddNotificationHandler(typeof(GenericNotificationHandler<>))
            .BuildServiceProvider();

        await provider
            .GetRequiredService<IMediator>()
            .Publish(new GenericNotification<string?>(expected));

        Assert.Equal(2, result.Values.Count);
        result.Values.Clear();

        await provider
            .GetRequiredService<IMediator>()
            .Publish(new GenericNotification<string?>(null));

        Assert.Equal(2, result.Values.Count);
    }

    [Fact]
    public async Task NoRegistration()
    {
        const string expected = "success";

        await using var provider = new ServiceCollection()
            .AddMediator()
            .BuildServiceProvider();

        await provider
            .GetRequiredService<IMediator>()
            .Publish(new GenericNotification<string?>(expected));
    }
}
