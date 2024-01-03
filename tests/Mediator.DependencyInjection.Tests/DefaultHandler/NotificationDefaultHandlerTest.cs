using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.DefaultHandler;

public class NotificationDefaultHandlerTest
{
	[Fact]
	public async Task TestNotification()
	{
		var handler = Substitute.For<IDefaultNotificationHandler>();

		var serviceProvider = new ServiceCollection()
			.AddMediator(b => b.AddDefaultNotificationHandler(handler))
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Publish(new Notification());

		_ = handler.Received()
			.Handle(Arg.Any<IServiceProvider>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>());
	}
}
