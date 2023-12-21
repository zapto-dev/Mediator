using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zapto.Mediator;

public delegate ValueTask<TResponse> RequestMiddleware<in TRequest, TResponse>(IServiceProvider provider, TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);

internal class DelegatePipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
	private readonly RequestMiddleware<TRequest, TResponse> _middleware;

	public DelegatePipelineBehavior(RequestMiddleware<TRequest, TResponse> middleware)
	{
		_middleware = middleware;
	}

	public ValueTask<TResponse> Handle(IServiceProvider provider, TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		return _middleware.Invoke(provider, request, next, cancellationToken);
	}
}