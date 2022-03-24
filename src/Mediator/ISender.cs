using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

/// <summary>
/// Send a request through the mediator pipeline to be handled by a single handler.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    ValueTask<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <param name="ns">Namespace of the request</param>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    ValueTask<TResponse> Send<TRequest, TResponse>(MediatorNamespace ns, TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Create a stream via a single stream handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns></returns>
    IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>;

    /// <summary>
    /// Create a stream via a single stream handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <param name="ns">Namespace of the stream</param>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns></returns>
    IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(MediatorNamespace ns, TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>;
}
