using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.DefaultHandler;

public class StreamRequestDefaultHandlerTest
{
	[Fact]
	public async Task TestStream()
	{
		var handler = Substitute.For<IDefaultStreamRequestHandler>();

		handler.Handle<StreamRequest, int>(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>())
			.Returns(Array.Empty<int>().ToAsyncEnumerable());

		var serviceProvider = new ServiceCollection()
			.AddMediator(b => b.AddDefaultStreamRequestHandler(handler))
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator
			.CreateStream<StreamRequest, int>(new StreamRequest())
			.ToListAsync();

		_ = handler.Received()
			.Handle<StreamRequest, int>(Arg.Any<IServiceProvider>(), Arg.Any<StreamRequest>(), Arg.Any<CancellationToken>());
	}
}
