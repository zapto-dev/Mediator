using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Mediator.DependencyInjection.Tests;

public record StreamRequest : IStreamRequest<int>;

public class StreamRequestTest
{
    [Fact]
    public async Task TestStream()
    {
        var handler = new Mock<IStreamRequestHandler<StreamRequest, int>>();

        handler.Setup(h => h.Handle(It.IsAny<IServiceProvider>(), It.IsAny<StreamRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Array.Empty<int>().ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddStreamRequestHandler(handler.Object)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator
            .CreateStream<StreamRequest, int>(new StreamRequest())
            .ToListAsync();

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<StreamRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TestStreamInterface()
    {
        var handler = new Mock<IStreamRequestHandler<StreamRequest, int>>();

        handler.Setup(h => h.Handle(It.IsAny<IServiceProvider>(), It.IsAny<StreamRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Array.Empty<int>().ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddStreamRequestHandler(handler.Object)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator
            .CreateStream(new StreamRequest())
            .ToListAsync();

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<StreamRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TestStreamObject()
    {
        var handler = new Mock<IStreamRequestHandler<StreamRequest, int>>();

        handler.Setup(h => h.Handle(It.IsAny<IServiceProvider>(), It.IsAny<StreamRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Array.Empty<int>().ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddStreamRequestHandler(handler.Object)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator
            .CreateStream((object) new StreamRequest())
            .ToListAsync();

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<StreamRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TestNamespaceStream()
    {
        var ns = new MediatorNamespace("test");
        var handler = new Mock<IStreamRequestHandler<StreamRequest, int>>();

        handler.Setup(h => h.Handle(It.IsAny<IServiceProvider>(), It.IsAny<StreamRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Array.Empty<int>().ToAsyncEnumerable());

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddStreamRequestHandler(handler.Object)
            .AddStreamRequestHandler(handler.Object, ns)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator
            .CreateStream<StreamRequest, int>(new StreamRequest())
            .ToListAsync();

        await mediator
            .CreateStream<StreamRequest, int>(ns, new StreamRequest())
            .ToListAsync();

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<StreamRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
