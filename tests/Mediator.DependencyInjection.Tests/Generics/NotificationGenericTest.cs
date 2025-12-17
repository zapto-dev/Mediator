using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class NotificationGenericTest
{
	[Fact]
	public async Task Valid()
	{
		const string expected = "success";
		var result = new Result();

		await using var provider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddNotificationHandler(typeof(GenericNotificationHandler<>));
				b.AddNotificationHandler(typeof(GenericNotificationHandler<>));
				b.AddNotificationHandler<StringGenericNotificationHandler>();
			})
			.AddSingleton(result)
			.BuildServiceProvider();

		await provider
			.GetRequiredService<IMediator>()
			.Publish(new GenericNotification<string?>(expected));

		Assert.Equal(3, result.Values.Count);
		result.Values.Clear();

		await provider
			.GetRequiredService<IMediator>()
			.Publish(new GenericNotification<string?>(null));

		Assert.Equal(3, result.Values.Count);
	}

	[Fact]
	public async Task NoRegistration()
	{
		const string expected = "success";

		await using var provider = new ServiceCollection()
			.AddMediator(_ => {})
			.BuildServiceProvider();

		await provider
			.GetRequiredService<IMediator>()
			.Publish(new GenericNotification<string?>(expected));
	}

	[Fact]
	public async Task TestGenericRegistration()
	{
		var handler = Substitute.For<INotificationHandler<GenericNotification<string>>>();

		var serviceProvider = new ServiceCollection()
			.AddMediator(b => b.AddNotificationHandler(handler))
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Publish(new GenericNotification<string>("test"));
		await mediator.Publish(new GenericNotification<int>(5));

		Assert.Single(handler.ReceivedCalls());
	}

	[Fact]
	public async Task TestGenericRegistrationNamespace()
	{
		var handler = Substitute.For<INotificationHandler<GenericNotification<string>>>();
		var ns = new MediatorNamespace("test");

		var serviceProvider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddNamespace(ns, inner =>
				{
					inner.AddNotificationHandler(handler);
				});
			})
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Publish(ns, new GenericNotification<string>("test"));
		await mediator.Publish(ns, new GenericNotification<int>(5));

		Assert.Single(handler.ReceivedCalls());
	}

	[Fact]
	public async Task TestNestedGenericRegistration()
	{
		var handler = Substitute.For<INotificationHandler<GenericNotification<Wrapper<string>>>>();

		var serviceProvider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddNotificationHandler(handler);
			})
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Publish(new GenericNotification<Wrapper<string>>("test"));
		await mediator.Publish(new GenericNotification<Wrapper<int>>(5));

		Assert.Single(handler.ReceivedCalls());
	}

	[Fact]
	public async Task TestNestedGenericRegistrationNamespace()
	{
		var handler = Substitute.For<INotificationHandler<GenericNotification<Wrapper<string>>>>();
		var ns = new MediatorNamespace("test");

		var serviceProvider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddNamespace(ns, inner =>
				{
					inner.AddNotificationHandler(handler);
				});
			})
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Publish(ns, new GenericNotification<Wrapper<string>>("test"));
		await mediator.Publish(ns, new GenericNotification<Wrapper<int>>(5));

		Assert.Single(handler.ReceivedCalls());
	}

	[Fact]
	public async Task TestGenericConstraintRegistrationNamespace()
	{
		var serviceProvider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddNotificationHandler(typeof(GenericNotificationHandlerConstraintInterfaceA<>));
			})
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Publish(new GenericNotification<ClassImplementingA>(new ClassImplementingA()));
		await mediator.Publish(new GenericNotification<ClassImplementingB>(new ClassImplementingB()));
	}

	public class GenericNotificationHandlerConstraintInterfaceA<T>
		: INotificationHandler<GenericNotification<T>> where T : IInterfaceA
	{
		public int Count { get; set; }

		public ValueTask Handle(IServiceProvider provider, GenericNotification<T> notification, CancellationToken cancellationToken)
		{
			Count++;
			return default;
		}
	}

	[Fact]
	public async Task TestGenericDirect()
	{
		var serviceProvider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddNotificationHandler<NotificationHandlerDirect>();
			})
			.BuildServiceProvider();

		var mediator = serviceProvider.GetRequiredService<IMediator>();

		await mediator.Publish(new GenericNotification<ClassImplementingA>(new ClassImplementingA()));
		await mediator.Publish(new GenericNotification<ClassImplementingB>(new ClassImplementingB()));
	}

	public class NotificationHandlerDirect : INotificationHandler<GenericNotification<ClassImplementingA>>
	{
		public int Count { get; set; }

		public ValueTask Handle(IServiceProvider provider, GenericNotification<ClassImplementingA> notification, CancellationToken cancellationToken)
		{
			Count++;
			return default;
		}
	}
}
