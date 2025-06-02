using System.Threading.Tasks;
using Mediator.DependencyInjection.Tests.Delegates;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Pipeline;

public class RequestPipelineTest
{
	[Fact]
	public async Task SinglePipeline()
	{
		const string input = "success";
		const string expected = "SUCCESS";

		await using var provider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddRequestHandler((StringRequest _) => input);
				b.AddPipelineBehavior<StringRequest, string>(async (serviceProvider, request, next, token) =>
				{
					return (await next()).ToUpperInvariant();
				});
			})
			.BuildServiceProvider();

		Assert.Equal(
			expected,
			await provider.GetRequiredService<IMediator>().Send<StringRequest, string>(new StringRequest())
		);
	}

	[Fact]
	public async Task MultiplePipeline()
	{
		const string input = "success";
		const string expected = "SUCCESS!";

		await using var provider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddRequestHandler((StringRequest _) => input);

				b.AddPipelineBehavior<StringRequest, string>(async (serviceProvider, request, next, token) =>
				{
					return (await next()).ToUpperInvariant();
				});

				b.AddPipelineBehavior<StringRequest, string>(async (serviceProvider, request, next, token) =>
				{
					return await next() + "!";
				});
			})
			.BuildServiceProvider();

		Assert.Equal(
			expected,
			await provider.GetRequiredService<IMediator>().Send<StringRequest, string>(new StringRequest())
		);
	}

	[Fact]
	public async Task UnitPipeline()
	{
		var requestHandlerCalled = false;
		var pipelineBehaviorCalled = false;

		await using var provider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddRequestHandler((VoidRequest _) =>
				{
					requestHandlerCalled = true;
				});
				b.AddPipelineBehavior<VoidRequest>(async (_, _, next, _) =>
				{
					pipelineBehaviorCalled = true;
					return await next();
				});
			})
			.BuildServiceProvider();

		await provider.GetRequiredService<IMediator>().Send(new VoidRequest());

		Assert.True(requestHandlerCalled, "Request handler was not called");
		Assert.True(pipelineBehaviorCalled, "Pipeline behavior was not called");
	}
}
