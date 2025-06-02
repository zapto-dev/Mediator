                        
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

public record VoidRequest : IRequest;

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

        Assert.Empty(result);
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

    [Fact]
    public async Task TestVoidRequest()
    {
        var handler = Substitute.For<IRequestHandler<VoidRequest>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new VoidRequest());

        _ = handler.Received()
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<VoidRequest>(), Arg.Any<CancellationToken>());
    }

    private abstract class BaseRequestHandler<T> : IRequestHandler<T, int>
        where T : IRequest<int>
    {
        public ValueTask<int> Handle(IServiceProvider provider, T request, CancellationToken cancellationToken) => new(1);
    }

    private class RequestHandler : BaseRequestHandler<Request>;

    [Fact]
    public async Task TestBaseRequestHandler()
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(typeof(RequestHandler)))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new Request());

        Assert.Equal(1, result);
    }

    private abstract class BaseVoidRequestHandler<T> : IRequestHandler<T>
        where T : IRequest
    {
        public ValueTask Handle(IServiceProvider provider, T request, CancellationToken cancellationToken) => default;
    }

    private class BaseVoidRequestHandler : BaseVoidRequestHandler<VoidRequest>;

    [Fact]
    public async Task TestBaseVoidRequestHandler()
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(typeof(BaseVoidRequestHandler)))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new VoidRequest());
    }
}
