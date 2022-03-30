using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;

namespace Zapto.Mediator;

/// <summary>
/// Defines a handler for a stream request using IAsyncEnumerable as return type.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public interface IStreamRequestHandler<in TRequest, out TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Handles a stream request with IAsyncEnumerable as return type.
    /// </summary>
    /// <param name="provider">Current service provider</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
    IAsyncEnumerable<TResponse> Handle(IServiceProvider provider, TRequest request, CancellationToken cancellationToken);
}
