#if NET7_0_OR_GREATER
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public record struct ReturnNumberRequest<TSelf>(TSelf Value) : IRequest<TSelf>
	where TSelf : INumber<TSelf>;

public class ReturnNumberRequestHandler<TSelf> : IRequestHandler<ReturnNumberRequest<TSelf>, TSelf>
	where TSelf : INumber<TSelf>
{
	public ValueTask<TSelf> Handle(IServiceProvider provider, ReturnNumberRequest<TSelf> request,
		CancellationToken cancellationToken)
	{
		return new ValueTask<TSelf>(request.Value);
	}
}

public class AddOnePipelineBehavior<TSelf> : IPipelineBehavior<ReturnNumberRequest<TSelf>, TSelf>
	where TSelf : INumber<TSelf>, IAdditionOperators<TSelf, TSelf, TSelf>
{
	public async ValueTask<TSelf> Handle(IServiceProvider provider, ReturnNumberRequest<TSelf> request, RequestHandlerDelegate<TSelf> next,
		CancellationToken cancellationToken)
	{
		var result = await next();

		return result + TSelf.One;
	}
}

public class BehaviorNumberTest
{
	[Fact]
	public async Task ReturnSelf()
	{
		await using var provider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddRequestHandler(typeof(ReturnNumberRequestHandler<>));
			})
			.BuildServiceProvider();

		var mediator = provider.GetRequiredService<IMediator>();

		Assert.Equal(0, await mediator.Send(new ReturnNumberRequest<int>(0)));
		Assert.Equal(10L, await mediator.Send(new ReturnNumberRequest<long>(10L)));
	}

	[Fact]
	public async Task AddOneBehavior()
	{
		await using var provider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddRequestHandler(typeof(ReturnNumberRequestHandler<>));
				b.AddPipelineBehavior(typeof(AddOnePipelineBehavior<>));
			})
			.BuildServiceProvider();

		var mediator = provider.GetRequiredService<IMediator>();

		Assert.Equal(1, await mediator.Send(new ReturnNumberRequest<int>(0)));
		Assert.Equal(11L, await mediator.Send(new ReturnNumberRequest<long>(10L)));
	}
}
#endif