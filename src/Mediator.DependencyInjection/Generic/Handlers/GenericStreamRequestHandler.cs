using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal sealed class GenericStreamRequestRegistration
{
    public GenericStreamRequestRegistration(Type requestType, Type? responseType, Type handlerType)
    {
        RequestType = requestType;
        ResponseType = responseType;
        HandlerType = handlerType;
    }

    public Type RequestType { get; }

    public Type? ResponseType { get; }

    public Type HandlerType { get; }
}

internal sealed class GenericStreamRequestCache<TRequest, TResponse>
{
    public Type? RequestHandlerType { get; set; }

    public List<GenericStreamRequestRegistration>? MatchingRegistrations { get; set; }
}

internal sealed class GenericStreamRequestHandler<TRequest, TResponse> : IStreamRequestHandler<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly GenericStreamRequestCache<TRequest, TResponse> _cache;
    private readonly IEnumerable<GenericStreamRequestRegistration> _enumerable;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDefaultStreamRequestHandler? _defaultHandler;

    public GenericStreamRequestHandler(
        IEnumerable<GenericStreamRequestRegistration> enumerable,
        IServiceProvider serviceProvider,
        GenericStreamRequestCache<TRequest, TResponse> cache,
        IDefaultStreamRequestHandler? defaultHandler = null)
    {
        _enumerable = enumerable;
        _serviceProvider = serviceProvider;
        _cache = cache;
        _defaultHandler = defaultHandler;
    }

    public IAsyncEnumerable<TResponse> Handle(IServiceProvider provider, TRequest request, CancellationToken ct)
    {
        if (_cache.RequestHandlerType is {} cachedType)
        {
            var handler = _serviceProvider.GetRequiredService(cachedType);

            return ((IStreamRequestHandler<TRequest, TResponse>)handler).Handle(provider, request, ct);
        }

        var requestType = typeof(TRequest);
        var responseType = typeof(TResponse);

        if (!requestType.IsGenericType)
        {
            if (_defaultHandler is null)
            {
                throw new HandlerNotFoundException($"No handler found for request type {requestType.FullName}.");
            }

            return _defaultHandler.Handle<TRequest, TResponse>(provider, request, ct);
        }

        var arguments = requestType.GetGenericArguments();

        requestType = requestType.GetGenericTypeDefinition();

        if (responseType.IsGenericType)
        {
            responseType = responseType.GetGenericTypeDefinition();
        }

        // Cache matching registrations on first call to avoid enumerating on every Handle call
        if (_cache.MatchingRegistrations == null)
        {
            _cache.MatchingRegistrations = GenericTypeHelper.CacheMatchingRegistrations(
                _enumerable,
                r => r.RequestType,
                requestType);
        }

        foreach (var registration in _cache.MatchingRegistrations)
        {
            if (registration.ResponseType is not null && registration.ResponseType != responseType)
            {
                continue;
            }

            Type type;
            if (registration.HandlerType.IsGenericType)
            {
                // Check if the generic arguments satisfy the handler's constraints
                if (!GenericTypeHelper.CanMakeGenericType(registration.HandlerType, arguments))
                {
                    continue;
                }

                type = registration.HandlerType.MakeGenericType(arguments);
            }
            else
            {
                type = registration.HandlerType;
            }

            var handler = (IStreamRequestHandler<TRequest, TResponse>) _serviceProvider.GetRequiredService(type);

            _cache.RequestHandlerType = type;

            return handler.Handle(provider, request, ct);
        }

        if (_defaultHandler is null)
        {
            throw new HandlerNotFoundException($"No handler found for request type {requestType.FullName}.");
        }

        return _defaultHandler.Handle<TRequest, TResponse>(provider, request, ct);
    }
}
