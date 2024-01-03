using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;

namespace Zapto.Mediator;

public interface IDefaultStreamRequestHandler
{
	/// <summary>
	/// Handles a stream request with IAsyncEnumerable as return type.
	/// </summary>
	/// <param name="provider">Current service provider</param>
	/// <param name="request">The request</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Response from the request</returns>
	IAsyncEnumerable<TResponse> Handle<TRequest, TResponse>(IServiceProvider provider, TRequest request, CancellationToken cancellationToken)
		where TRequest : IStreamRequest<TResponse>;
}
