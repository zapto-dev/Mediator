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
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    ValueTask Send(IRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    ValueTask Send(MediatorNamespace ns, IRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    ValueTask<TResponse> Send<TResponse>(MediatorNamespace ns, IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send an object request to a single handler via dynamic dispatch
    /// </summary>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the type erased handler response</returns>
    ValueTask<object?> Send(object request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send an object request to a single handler via dynamic dispatch
    /// </summary>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the type erased handler response</returns>
    ValueTask<object?> Send(MediatorNamespace ns, object request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;

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
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <param name="ns">Namespace of the request</param>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    ValueTask Send<TRequest>(MediatorNamespace ns, TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;

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
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a stream via a single stream handler
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(MediatorNamespace ns, IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a stream via an object request to a stream handler
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a stream via an object request to a stream handler
    /// </summary>
    /// <param name="ns">Namespace of the notification</param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<object?> CreateStream(MediatorNamespace ns, object request, CancellationToken cancellationToken = default);

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
