using System.Threading.Tasks;
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
            .BuildServiceProvider();

        await provider
            .GetRequiredService<IMediator>()
            .Publish(new GenericNotification<string?>(expected));

        Assert.Equal(expected, result.Object);

        await provider
            .GetRequiredService<IMediator>()
            .Publish(new GenericNotification<string?>(null));

        Assert.Null(result.Object);
    }
}
