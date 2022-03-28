using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

internal sealed class GenericRequestCache
{
    public ConcurrentDictionary<(Type Request, Type Response), Type> Handlers { get; } = new();
}

internal sealed class GenericRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly GenericRequestCache _cache;
    private readonly IEnumerable<GenericRequestRegistration> _enumerable;
    private readonly IServiceProvider _serviceProvider;

    public GenericRequestHandler(IEnumerable<GenericRequestRegistration> enumerable, IServiceProvider serviceProvider, GenericRequestCache cache)
    {
        _enumerable = enumerable;
        _serviceProvider = serviceProvider;
        _cache = cache;
    }

    public async ValueTask<TResponse> Handle(TRequest request, CancellationToken ct)
    {
        var requestType = typeof(TRequest);
        var responseType = typeof(TResponse);
        var cacheKey = (requestType, responseType);

        if (_cache.Handlers.TryGetValue(cacheKey, out var handlerType))
        {
            var handler = _serviceProvider.GetRequiredService(handlerType);

            return await ((IRequestHandler<TRequest, TResponse>)handler).Handle(request, ct);
        }

        if (!requestType.IsGenericType)
        {
            throw new InvalidCastException($"No handler found for request type {requestType.FullName}.");
        }

        var arguments = requestType.GetGenericArguments();

        requestType = requestType.GetGenericTypeDefinition();

        if (responseType.IsGenericType)
        {
            responseType = responseType.GetGenericTypeDefinition();
        }

        foreach (var registration in _enumerable)
        {
            if (registration.RequestType != requestType ||
                registration.ResponseType is not null && registration.ResponseType != responseType)
            {
                continue;
            }

            var type = registration.HandlerType.MakeGenericType(arguments);
            var handler = (IRequestHandler<TRequest, TResponse>) _serviceProvider.GetRequiredService(type);

            _cache.Handlers.TryAdd(cacheKey, type);

            return await handler.Handle(request, ct);
        }

        throw new InvalidCastException($"No handler found for request type {requestType.FullName}.");
    }
}
