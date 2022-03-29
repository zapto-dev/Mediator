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

public record Request : IRequest<int>;

public class RequestTest
{
    [Fact]
    public async Task TestRequest()
    {
        var handler = new Mock<IRequestHandler<Request, int>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddRequestHandler(handler.Object)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send<Request, int>(new Request());

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TestNamespaceRequest()
    {
        var ns = new MediatorNamespace("test");
        var handler = new Mock<IRequestHandler<Request, int>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator()
            .AddRequestHandler(handler.Object)
            .AddRequestHandler(handler.Object, ns)
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send<Request, int>(new Request());
        await mediator.Send<Request, int>(ns, new Request());

        handler.Verify(x => x.Handle(It.IsAny<IServiceProvider>(), It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
