using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal sealed class GenericRequestRegistration
{
    public GenericRequestRegistration(Type requestType, Type? responseType, Type handlerType)
    {
        RequestType = requestType;
        ResponseType = responseType;
        HandlerType = handlerType;
    }

    public Type RequestType { get; }

    public Type? ResponseType { get; }

    public Type HandlerType { get; }
}

internal sealed class GenericRequestCache<TRequest, TResponse>
{
    public Type? RequestHandlerType { get; set; }

    public List<GenericRequestRegistration>? MatchingRegistrations { get; set; }
}

internal sealed class GenericRequestCache<TRequest>
{
    public Type? RequestHandlerType { get; set; }

    public List<GenericRequestRegistration>? MatchingRegistrations { get; set; }
}

internal sealed class GenericRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly GenericRequestCache<TRequest, TResponse> _cache;
    private readonly IEnumerable<GenericRequestRegistration> _enumerable;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDefaultRequestHandler? _defaultHandler;

    public GenericRequestHandler(
        IEnumerable<GenericRequestRegistration> enumerable,
        IServiceProvider serviceProvider,
        GenericRequestCache<TRequest, TResponse> cache,
        IDefaultRequestHandler? defaultHandler = null)
    {
        _enumerable = enumerable;
        _serviceProvider = serviceProvider;
        _cache = cache;
        _defaultHandler = defaultHandler;
    }

    public async ValueTask<TResponse> Handle(IServiceProvider provider, TRequest request, CancellationToken ct)
    {
        if (_cache.RequestHandlerType is {} cachedType)
        {
            var handler = _serviceProvider.GetRequiredService(cachedType);

            return await ((IRequestHandler<TRequest, TResponse>)handler).Handle(provider, request, ct);
        }

        var requestType = typeof(TRequest);
        var responseType = typeof(TResponse);

        if (!requestType.IsGenericType)
        {
            if (_defaultHandler is null)
            {
                throw new HandlerNotFoundException($"No handler found for request type {requestType.FullName}.");
            }

            return await _defaultHandler.Handle<TRequest, TResponse>(_serviceProvider, request, ct);
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

            var handler = (IRequestHandler<TRequest, TResponse>) _serviceProvider.GetRequiredService(type);

            _cache.RequestHandlerType = type;

            return await handler.Handle(provider, request, ct);
        }

        if (_defaultHandler is null)
        {
            throw new HandlerNotFoundException($"No handler found for request type {requestType.FullName}.");
        }

        var method = _defaultHandler
            .GetType()
            .GetMethods()
            .First(m => m.Name == nameof(IDefaultRequestHandler.Handle) && m.GetGenericArguments().Length == 2)
            .MakeGenericMethod(arguments);

        return await (ValueTask<TResponse>) method.Invoke(_defaultHandler, new object[] {_serviceProvider, request, ct})!;
    }
}


internal sealed class GenericRequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    private readonly GenericRequestCache<TRequest> _cache;
    private readonly IEnumerable<GenericRequestRegistration> _enumerable;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDefaultRequestHandler? _defaultHandler;

    public GenericRequestHandler(
        IEnumerable<GenericRequestRegistration> enumerable,
        IServiceProvider serviceProvider,
        GenericRequestCache<TRequest> cache,
        IDefaultRequestHandler? defaultHandler = null)
    {
        _enumerable = enumerable;
        _serviceProvider = serviceProvider;
        _cache = cache;
        _defaultHandler = defaultHandler;
    }

    public async ValueTask Handle(IServiceProvider provider, TRequest request, CancellationToken ct)
    {
        if (_cache.RequestHandlerType is {} cachedType)
        {
            var handler = _serviceProvider.GetRequiredService(cachedType);

            await ((IRequestHandler<TRequest>)handler).Handle(provider, request, ct);
            return;
        }

        var requestType = typeof(TRequest);

        if (!requestType.IsGenericType)
        {
            if (_defaultHandler is null)
            {
                throw new HandlerNotFoundException($"No handler found for request type {requestType.FullName}.");
            }

            await _defaultHandler.Handle(_serviceProvider, request, ct);
            return;
        }

        var arguments = requestType.GetGenericArguments();

        requestType = requestType.GetGenericTypeDefinition();

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
            if (registration.ResponseType != typeof(Unit))
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

            var handler = (IRequestHandler<TRequest>) _serviceProvider.GetRequiredService(type);

            _cache.RequestHandlerType = type;

            await handler.Handle(provider, request, ct);
            return;
        }

        if (_defaultHandler is null)
        {
            throw new HandlerNotFoundException($"No handler found for request type {requestType.FullName}.");
        }

        var method = _defaultHandler
            .GetType()
            .GetMethods()
            .First(m => m.Name == nameof(IDefaultRequestHandler.Handle) && m.GetGenericArguments().Length == 1)
            .MakeGenericMethod(arguments);

         await (ValueTask) method.Invoke(_defaultHandler, new object[] {_serviceProvider, request, ct})!;
    }
}