using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Mediator.DependencyInjection.Tests;

public record StreamRequest : IStreamRequest<int>;

public class StreamRequestTest
{
    [Fact]
    public async Task TestStream()
    {
        var handler = Substitute.For<IStreamRequestHandler<StreamRequest, int>>();

        handler.Handle(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<int>().ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddStreamRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator
            .CreateStream<StreamRequest, int>(new StreamRequest())
            .ToListAsync();

        _ = handler.Received()
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestStreamInterface()
    {
        var handler = Substitute.For<IStreamRequestHandler<StreamRequest, int>>();

        handler.Handle(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<int>().ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddStreamRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator
            .CreateStream(new StreamRequest())
            .ToListAsync();

        _ = handler.Received()
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestStreamObject()
    {
        var handler = Substitute.For<IStreamRequestHandler<StreamRequest, int>>();

        handler.Handle(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<int>().ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddStreamRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator
            .CreateStream((object) new StreamRequest())
            .ToListAsync();

        _ = handler.Received()
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestNamespaceStream()
    {
        var ns = new MediatorNamespace("test");
        var handler = Substitute.For<IStreamRequestHandler<StreamRequest, int>>();

        handler.Handle(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<int>().ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddStreamRequestHandler(handler);
                b.AddNamespace(ns).AddStreamRequestHandler(handler);
            })
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator
            .CreateStream<StreamRequest, int>(new StreamRequest())
            .ToListAsync();

        await mediator
            .CreateStream<StreamRequest, int>(ns, new StreamRequest())
            .ToListAsync();

        _ = handler.Received(requiredNumberOfCalls: 2)
            .Handle(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>());
    }
}
