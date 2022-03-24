using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal class GenericRegistration
{
    public GenericRegistration(Type requestType, Type responseType, Type handlerType)
    {
        RequestType = requestType;
        ResponseType = responseType;
        HandlerType = handlerType;
    }

    public Type RequestType { get; }

    public Type ResponseType { get; }

    public Type HandlerType { get; }
}

internal class GenericRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<GenericRegistration> _enumerable;
    private readonly IServiceProvider _serviceProvider;

    public GenericRequestHandler(IEnumerable<GenericRegistration> enumerable, IServiceProvider serviceProvider)
    {
        _enumerable = enumerable;
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<TResponse> Handle(TRequest request, CancellationToken ct)
    {
        var requestType = typeof(TRequest);
        var responseType = typeof(TResponse);

        if (!requestType.IsGenericType)
        {
            throw new InvalidOperationException();
        }

        var arguments = requestType.GetGenericArguments();

        requestType = requestType.GetGenericTypeDefinition();

        foreach (var registration in _enumerable)
        {
            if (registration.RequestType != requestType || registration.ResponseType != responseType)
            {
                continue;
            }

            var handler = (IRequestHandler<TRequest, TResponse>) _serviceProvider.GetRequiredService(
                registration.HandlerType.MakeGenericType(arguments)
            );

            return await handler.Handle(request, ct);
        }

        throw new InvalidOperationException();
    }
}
