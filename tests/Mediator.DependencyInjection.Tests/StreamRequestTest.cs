using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mediator.DependencyInjection.Tests;

public record StreamRequest : IStreamRequest<int>;

public class StreamRequestTest
{
    [Fact]
    public async Task TestStream()
    {
        const int expected = 1;

        async IAsyncEnumerable<int> Test(StreamRequest a)
        {
            await Task.Yield();
            yield return expected;
        }

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddStreamRequestHandler<StreamRequest, int>(Test)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var result = await mediator
            .CreateStream<StreamRequest, int>(new StreamRequest())
            .FirstOrDefaultAsync();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task TestNamespaceStream()
    {
        var ns = new MediatorNamespace("test");
        const int expectedGlobal = 1;
        const int expectedNs = 2;

        async IAsyncEnumerable<int> TestGlobal(StreamRequest a)
        {
            await Task.Yield();
            yield return expectedGlobal;
        }

        async IAsyncEnumerable<int> TestNamespace(StreamRequest a)
        {
            await Task.Yield();
            yield return expectedNs;
        }

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddStreamRequestHandler<StreamRequest, int>(TestGlobal)
            .AddStreamRequestHandler<StreamRequest, int>(TestNamespace, ns)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var resultGlobal = await mediator
            .CreateStream<StreamRequest, int>(new StreamRequest())
            .FirstOrDefaultAsync();

        var resultNs = await mediator
            .CreateStream<StreamRequest, int>(ns, new StreamRequest())
            .FirstOrDefaultAsync();

        Assert.Equal(expectedGlobal, resultGlobal);
        Assert.Equal(expectedNs, resultNs);
    }
}
