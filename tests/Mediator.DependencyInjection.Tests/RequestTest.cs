                        
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Mediator.DependencyInjection.Tests;

public record Request : IRequest<int>;

public record ListRequest : IRequest<IReadOnlyList<int>>;

public class ListHandler : IRequestHandler<ListRequest, IReadOnlyList<int>>
{
    public ValueTask<IReadOnlyList<int>> Handle(IServiceProvider provider, ListRequest request, CancellationToken cancellationToken)
        => new(Array.Empty<int>());
}

public class RequestTest
{
    [Fact]
    public async Task TestRequest()
    {
        var handler = Substitute.For<IRequestHandler<Request, int>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send<Request, int>(new Request());

        _ = handler.Received()
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestRequestInterface()
    {
        var handler = Substitute.For<IRequestHandler<Request, int>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new Request());

        _ = handler.Received()
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestRequestObject()
    {
        var handler = Substitute.For<IRequestHandler<Request, int>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send((object)new Request());

        _ = handler.Received()
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestNamespaceRequest()
    {
        var ns = new MediatorNamespace("test");
        var handler = Substitute.For<IRequestHandler<Request, int>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddRequestHandler(handler);
                b.AddNamespace(ns).AddRequestHandler(handler);
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send<Request, int>(new Request());
        await mediator.Send<Request, int>(ns, new Request());

        _ = handler.Received(requiredNumberOfCalls: 2)
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<Request>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestCollection()
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(typeof(ListHandler)))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var result = await mediator.Send<ListRequest, IReadOnlyList<int>>(new ListRequest());

        Assert.Equal(0, result.Count);
    }

    [Fact]
    public async Task TestNoHandler()
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(_ => {})
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<HandlerNotFoundException>(() => mediator.Send<Request, int>(new Request()).AsTask());
    }

    [Fact]
    public async Task TestNoHandlerNamespace()
    {
        var ns = new MediatorNamespace("test");

        var serviceProvider = new ServiceCollection()
            .AddMediator(_ => {})
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<NamespaceHandlerNotFoundException>(() => mediator.Send<Request, int>(ns, new Request()).AsTask());
    }
}
