using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Zapto.Mediator;

public delegate IAsyncEnumerable<TResponse> StreamMiddleware<in TRequest, TResponse>(IServiceProvider provider, TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken);

internal class DelegateStreamPipelineBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
	private readonly StreamMiddleware<TRequest, TResponse> _middleware;

	public DelegateStreamPipelineBehavior(StreamMiddleware<TRequest, TResponse> middleware)
	{
		_middleware = middleware;
	}

	public IAsyncEnumerable<TResponse> Handle(IServiceProvider provider, TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		return _middleware.Invoke(provider, request, next, cancellationToken);
	}
}