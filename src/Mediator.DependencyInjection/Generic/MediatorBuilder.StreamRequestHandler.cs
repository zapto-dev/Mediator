using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddStreamRequestHandler(Type handlerType)
    {
        var handlers = handlerType.GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>))
            .ToArray();

        foreach (var type in handlers)
        {
            var requestType = type.GetGenericArguments()[0];
            var responseType = type.GetGenericArguments()[1];

            if (requestType.IsGenericType)
            {
                requestType = requestType.GetGenericTypeDefinition();
            }

            AddStreamRequestHandler(
                requestType,
                responseType switch
                {
                    { IsGenericParameter: true } => null,
                    { IsGenericType: true } when HasGenericParameter(responseType) => responseType.GetGenericTypeDefinition(),
                    _ => responseType
                },
                handlerType);
        }

        return this;
    }

    public IMediatorBuilder AddStreamRequestHandler<THandler>() where THandler : IStreamRequestHandler
    {
        return AddStreamRequestHandler(typeof(THandler));
    }

    public IMediatorBuilder AddStreamRequestHandler(
        Type requestType,
        Type? responseType,
        Type handlerType)
    {
        if (requestType.IsGenericType || responseType is null || responseType.IsGenericTypeDefinition)
        {
            _services.AddTransient(handlerType);
            _services.AddSingleton(new GenericStreamRequestRegistration(requestType, responseType, handlerType));
        }
        else
        {
            _services.AddTransient(typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, responseType), handlerType);
        }

        return this;
    }
}
