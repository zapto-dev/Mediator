﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

public interface IRequestHandler;

/// <summary>
/// Defines a handler for a request
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles a request
    /// </summary>
    /// <param name="provider">Current service provider</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
    ValueTask<TResponse> Handle(IServiceProvider provider, TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Defines a handler for a request with a void (<see cref="Unit" />) response.
/// You do not need to register this interface explicitly with a container as it inherits from the base <see cref="IRequestHandler{TRequest, TResponse}" /> interface.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Handles a request
    /// </summary>
    /// <param name="provider">Current service provider</param>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask Handle(IServiceProvider provider, TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Wrapper class for a handler that asynchronously handles a request and does not return a response
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
public abstract class AsyncRequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    async ValueTask IRequestHandler<TRequest>.Handle(IServiceProvider provider, TRequest request, CancellationToken cancellationToken)
    {
        await Handle(provider, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Override in a derived class for the handler logic
    /// </summary>
    /// <param name="serviceProvider">Current service provider</param>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Response</returns>
    protected abstract ValueTask Handle(IServiceProvider serviceProvider, TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Wrapper class for a handler that synchronously handles a request and returns a response
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> IRequestHandler<TRequest, TResponse>.Handle(IServiceProvider provider, TRequest request,
        CancellationToken cancellationToken)
        => new(Handle(provider, request));

    /// <summary>
    /// Override in a derived class for the handler logic
    /// </summary>
    /// <param name="provider">Current service provider</param>
    /// <param name="request">Request</param>
    /// <returns>Response</returns>
    protected abstract TResponse Handle(IServiceProvider provider, TRequest request);
}

/// <summary>
/// Wrapper class for a handler that synchronously handles a request does not return a response
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
public abstract class RequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    ValueTask IRequestHandler<TRequest>.Handle(IServiceProvider provider, TRequest request,
        CancellationToken cancellationToken)
    {
        Handle(provider, request);
        return default;
    }

    protected abstract void Handle(IServiceProvider provider, TRequest request);
}
