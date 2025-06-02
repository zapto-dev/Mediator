using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zapto.Mediator;

public delegate ValueTask<TResponse> RequestMiddleware<in TRequest, TResponse>(IServiceProvider provider, TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);

internal class DelegatePipelineBehavior<TRequest, TResponse>(RequestMiddleware<TRequest, TResponse> middleware) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	public ValueTask<TResponse> Handle(IServiceProvider provider, TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		return middleware.Invoke(provider, request, next, cancellationToken);
	}
}