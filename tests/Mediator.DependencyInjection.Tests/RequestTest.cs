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

public record Request : IRequest<int>;

public class RequestTest
{
    [Fact]
    public async Task TestRequest()
    {
        const int expected = 1;

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddRequestHandler((Request _) => expected)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send<Request, int>(new Request());

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task TestNamespaceRequest()
    {
        var ns = new MediatorNamespace("test");
        const int expectedGlobal = 1;
        const int expectedNs = 2;

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddRequestHandler((Request _) => expectedGlobal)
            .AddRequestHandler((Request _) => expectedNs, ns)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var resultGlobal = await mediator.Send<Request, int>(new Request());
        var resultNs = await mediator.Send<Request, int>(ns, new Request());

        Assert.Equal(expectedGlobal, resultGlobal);
        Assert.Equal(expectedNs, resultNs);
    }
}
