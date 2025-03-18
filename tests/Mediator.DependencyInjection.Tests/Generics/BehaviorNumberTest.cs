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

public record struct ReturnNumberRequest<TSelf>(TSelf Value) : IRequest<TSelf>, IAddOneInterface
	where TSelf : INumber<TSelf>;

public interface IAddOneInterface;

/// <summary>
/// Returns the value of the request.
/// </summary>
public class ReturnNumberRequestHandler<TSelf> : IRequestHandler<ReturnNumberRequest<TSelf>, TSelf>
	where TSelf : INumber<TSelf>
{
	public ValueTask<TSelf> Handle(IServiceProvider provider, ReturnNumberRequest<TSelf> request,
		CancellationToken cancellationToken)
	{
		return new ValueTask<TSelf>(request.Value);
	}
}

/// <summary>
/// Always adds one to the specific request.
/// </summary>
public class AddOneWithRequestPipelineBehavior<TSelf> : IPipelineBehavior<ReturnNumberRequest<TSelf>, TSelf>
	where TSelf : INumber<TSelf>, IAdditionOperators<TSelf, TSelf, TSelf>
{
	public async ValueTask<TSelf> Handle(IServiceProvider provider, ReturnNumberRequest<TSelf> request, RequestHandlerDelegate<TSelf> next,
		CancellationToken cancellationToken)
	{
		var result = await next();

		return result + TSelf.One;
	}
}

/// <summary>
/// Always adds one to the result if the response implements <see cref="INumber{T}"/>.
/// </summary>
public class AddOneWithResponsePipelineBehavior<TRequest, TSelf>  : IPipelineBehavior<TRequest, TSelf>
	where TRequest : IRequest<TSelf>
	where TSelf : INumber<TSelf>, IAdditionOperators<TSelf, TSelf, TSelf>
{
	public async ValueTask<TSelf> Handle(IServiceProvider provider, TRequest request, RequestHandlerDelegate<TSelf> next,
		CancellationToken cancellationToken)
	{
		var result = await next();

		return result + TSelf.One;
	}
}

/// <summary>
/// Add one from the result when the request implements <see cref="IAddOneInterface"/>.
/// </summary>
public class AddOneWithRequestInterfacePipelineBehavior<TRequest, TSelf> : IPipelineBehavior<TRequest, TSelf>
	where TRequest : IRequest<TSelf>, IAddOneInterface
	where TSelf : INumber<TSelf>
{
	public async ValueTask<TSelf> Handle(IServiceProvider provider, TRequest request, RequestHandlerDelegate<TSelf> next,
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

	[Theory]
	[InlineData(typeof(AddOneWithRequestPipelineBehavior<>))]
	[InlineData(typeof(AddOneWithResponsePipelineBehavior<,>))]
	[InlineData(typeof(AddOneWithRequestInterfacePipelineBehavior<,>))]
	public async Task AddOneBehaviorResponse(Type pipeline)
	{
		await using var provider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddRequestHandler(typeof(ReturnNumberRequestHandler<>));
				b.AddPipelineBehavior(pipeline);
			})
			.BuildServiceProvider();

		var mediator = provider.GetRequiredService<IMediator>();

		Assert.Equal(1, await mediator.Send(new ReturnNumberRequest<int>(0)));
		Assert.Equal(11L, await mediator.Send(new ReturnNumberRequest<long>(10L)));
	}
}
#endif