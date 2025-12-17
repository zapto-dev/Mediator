using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class RequestGenericTest
{
    [Fact]
    public async Task Valid()
    {
        const string expected = "success";

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddRequestHandler(typeof(ReturnGenericRequestHandler<>));
            })
            .BuildServiceProvider();

        var result = await provider
            .GetRequiredService<IMediator>()
            .Send<ReturnGenericRequest<string>, string>(new ReturnGenericRequest<string>(expected));

        Assert.Equal(expected, result);

        result = await provider
            .GetRequiredService<IMediator>()
            .Send<ReturnGenericRequest<string>, string>(new ReturnGenericRequest<string>(expected));

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ValidNonGeneric()
    {
        const string expected = "success";

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddRequestHandler<ReturnHandler>();
                b.AddRequestHandler(typeof(ReturnGenericRequestHandler<>));
            })
            .BuildServiceProvider();

        var genericResult = await provider
            .GetRequiredService<IMediator>()
            .Send<ReturnGenericRequest<string>, string>(new ReturnGenericRequest<string>(expected));

        Assert.Equal(expected, genericResult);

        var result = await provider
            .GetRequiredService<IMediator>()
            .Send<ReturnRequest, string>(new ReturnRequest(expected));

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ValidWrapped()
    {
        const int expected = 1;

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddRequestHandler(typeof(ReturnNullableGenericHandler<>));
            })
            .BuildServiceProvider();

        var result = await provider
            .GetRequiredService<IMediator>()
            .Send<ReturnNullableGenericRequest<int>, int?>(new ReturnNullableGenericRequest<int>(expected));

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task TestVoidRequest()
    {
        var handler = Substitute.For<IRequestHandler<GenericVoidRequest<string>>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new GenericVoidRequest<string>("Test"));

        _ = handler.Received()
            .Handle(Arg.Any<IServiceProvider>(), Arg.Is<GenericVoidRequest<string>>(r => r.Value == "Test"), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task TestVoidRequestHandler()
    {
        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(typeof(GenericVoidRequestHandler<>), RegistrationScope.Singleton))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new GenericVoidRequest<string>());

        var handler = serviceProvider.GetRequiredService<GenericVoidRequestHandler<string>>();

        Assert.True(handler.CallCount > 0, "Handler was not called");
    }

    [Fact]
    public async Task TestThrowNotFound()
    {
        var handler = Substitute.For<IRequestHandler<GenericVoidRequest<string>>>();

        var serviceProvider = new ServiceCollection()
            .AddMediator(b => b.AddRequestHandler(handler))
            .BuildServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<HandlerNotFoundException>(async () => await mediator.Send(new GenericVoidRequest<int>(1)));
    }
}
