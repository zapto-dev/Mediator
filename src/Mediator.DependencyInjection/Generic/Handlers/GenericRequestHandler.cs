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

internal sealed class GenericRequestCache<TRequest, TResponse>
{
    public Type? RequestHandlerType { get; set; }
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

        foreach (var registration in _enumerable)
        {
            if (registration.RequestType != requestType ||
                registration.ResponseType is not null && registration.ResponseType != responseType)
            {
                continue;
            }

            var type = registration.HandlerType.MakeGenericType(arguments);
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
            .GetMethod(nameof(IDefaultRequestHandler.Handle))!
            .MakeGenericMethod(arguments);

        return await (ValueTask<TResponse>) method.Invoke(_defaultHandler, new object[] {_serviceProvider, request, ct})!;

    }
}
