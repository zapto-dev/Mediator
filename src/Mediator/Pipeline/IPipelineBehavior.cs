using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zapto.Mediator;

/// <summary>
/// Represents an async continuation for the next task to execute in the pipeline
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
/// <returns>Awaitable task returning a <typeparamref name="TResponse"/></returns>
public delegate ValueTask<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Pipeline behavior to surround the inner handler.
/// Implementations add additional behavior and await the next delegate.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : notnull
{
	/// <summary>
	/// Pipeline handler. Perform any additional behavior and await the <paramref name="next"/> delegate as necessary
	/// </summary>
	/// <param name="provider">Service provider</param>
	/// <param name="request">Incoming request</param>
	/// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Awaitable task returning the <typeparamref name="TResponse"/></returns>
	ValueTask<TResponse> Handle(IServiceProvider provider, TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}