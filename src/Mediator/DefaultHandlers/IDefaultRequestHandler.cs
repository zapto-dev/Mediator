using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

public interface IDefaultRequestHandler
{
	/// <summary>
	/// Handles a request
	/// </summary>
	/// <param name="provider">Current service provider</param>
	/// <param name="request">The request</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Response from the request</returns>
	ValueTask<TResponse> Handle<TRequest, TResponse>(IServiceProvider provider, TRequest request, CancellationToken cancellationToken)
		where TRequest : IRequest<TResponse>;

	/// <summary>
	/// Handles a request
	/// </summary>
	/// <param name="provider">Current service provider</param>
	/// <param name="request">The request</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Response from the request</returns>
	ValueTask Handle<TRequest>(IServiceProvider provider, TRequest request, CancellationToken cancellationToken)
		where TRequest : IRequest;
}
