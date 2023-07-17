using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class StreamRequestGenericTest
{
    [Fact]
    public async Task Valid()
    {
        const string expected = "success";

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddStreamRequestHandler(typeof(ReturnGenericStreamRequestHandler<>));
            })
            .BuildServiceProvider();

        var result = await provider
            .GetRequiredService<IMediator>()
            .CreateStream<ReturnGenericStreamRequest<string>, string>(new ReturnGenericStreamRequest<string>(expected))
            .FirstOrDefaultAsync();

        Assert.Equal(expected, result);

        result = await provider
            .GetRequiredService<IMediator>()
            .CreateStream<ReturnGenericStreamRequest<string>, string>(new ReturnGenericStreamRequest<string>(expected))
            .FirstOrDefaultAsync();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ValidNonGeneric()
    {
        const string expected = "success";

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddStreamRequestHandler<ReturnStreamHandler>();
                b.AddStreamRequestHandler(typeof(ReturnGenericStreamRequestHandler<>));
            })
            .BuildServiceProvider();

        var genericResult = await provider
            .GetRequiredService<IMediator>()
            .CreateStream<ReturnGenericStreamRequest<string>, string>(new ReturnGenericStreamRequest<string>(expected))
            .FirstOrDefaultAsync();

        Assert.Equal(expected, genericResult);

        var result = await provider
            .GetRequiredService<IMediator>()
            .CreateStream<ReturnStreamRequest, string>(new ReturnStreamRequest(expected))
            .FirstOrDefaultAsync();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ValidWrapped()
    {
        const int expected = 1;

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddStreamRequestHandler(typeof(ReturnNullableGenericStreamHandler<>));
            })
            .BuildServiceProvider();

        var result = await provider
            .GetRequiredService<IMediator>()
            .CreateStream<ReturnNullableGenericStreamRequest<int>, int?>(new ReturnNullableGenericStreamRequest<int>(expected))
            .FirstOrDefaultAsync();

        Assert.Equal(expected, result);
    }
}
