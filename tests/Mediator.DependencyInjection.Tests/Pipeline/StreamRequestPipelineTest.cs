using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator.DependencyInjection.Tests.Delegates;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Pipeline;

public record StringStreamRequest(IEnumerable<string> Input) : IStreamRequest<string>;

public record StringStreamRequestHandler : IStreamRequestHandler<StringStreamRequest, string>
{
	public IAsyncEnumerable<string> Handle(IServiceProvider provider, StringStreamRequest request, CancellationToken cancellationToken)
	{
		return request.Input.ToAsyncEnumerable();
	}
}

public class StreamRequestPipelineTest
{
	[Fact]
	public async Task SinglePipeline()
	{
		const string input = "success";
		const string expected = "SUCCESS";

		await using var provider = new ServiceCollection()
			.AddMediator(b =>
			{
				b.AddStreamRequestHandler<StringStreamRequestHandler>();
				b.AddStreamPipelineBehavior<StringStreamRequest, string>((serviceProvider, request, next, token) =>
				{
					return next().Select(x => x.ToUpperInvariant());
				});
			})
			.BuildServiceProvider();

		Assert.Equal(
			new[] { expected },
			await provider.GetRequiredService<IMediator>()
				.CreateStream<StringStreamRequest, string>(new StringStreamRequest(new[] { input }))
				.ToArrayAsync()
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
				b.AddStreamRequestHandler<StringStreamRequestHandler>();

				b.AddStreamPipelineBehavior<StringStreamRequest, string>((serviceProvider, request, next, token) =>
				{
					return next().Select(x => x.ToUpperInvariant());
				});

				b.AddStreamPipelineBehavior<StringStreamRequest, string>((serviceProvider, request, next, token) =>
				{
					return next().Select(x => x + "!");
				});
			})
			.BuildServiceProvider();

		Assert.Equal(
			new[] { expected },
			await provider.GetRequiredService<IMediator>()
				.CreateStream<StringStreamRequest, string>(new StringStreamRequest(new[] { input }))
				.ToArrayAsync()
		);
	}
}
