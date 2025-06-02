using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.DefaultHandler;

public class RequestDefaultHandlerTest
{
	[Fact]
	public async Task TestRequest()
	{
		var handler = Substitute.For<IDefaultRequestHandler>();

		var serviceProvider = new ServiceCollection()
			.AddMediator(b => b.AddDefaultRequestHandler(handler))
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Send<Request, int>(new Request());

		_ = handler.Received()
			.Handle<Request, int>(Arg.Any<IServiceProvider>(), Arg.Any<Request>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task TestVoidRequest()
	{
		var handler = Substitute.For<IDefaultRequestHandler>();

		var serviceProvider = new ServiceCollection()
			.AddMediator(b => b.AddDefaultRequestHandler(handler))
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Send(new VoidRequest());

		_ = handler.Received()
			.Handle(Arg.Any<IServiceProvider>(), Arg.Any<VoidRequest>(), Arg.Any<CancellationToken>());
	}
}
