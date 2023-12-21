using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddStreamRequestHandler(Type handlerType, RegistrationScope scope = RegistrationScope.Transient)
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
                handlerType,
                scope);
        }

        return this;
    }

    public IMediatorBuilder AddStreamRequestHandler<THandler>(RegistrationScope scope = RegistrationScope.Transient) where THandler : IStreamRequestHandler
    {
        return AddStreamRequestHandler(typeof(THandler), scope);
    }

    public IMediatorBuilder AddStreamRequestHandler(
        Type requestType,
        Type? responseType,
        Type handlerType,
        RegistrationScope scope = RegistrationScope.Transient)
    {
        if (requestType.IsGenericType || responseType is null || responseType.IsGenericTypeDefinition)
        {
            _services.Add(new ServiceDescriptor(handlerType, handlerType, GetLifetime(scope)));
            _services.AddSingleton(new GenericStreamRequestRegistration(requestType, responseType, handlerType));
        }
        else
        {
            _services.Add(new ServiceDescriptor(typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, responseType), handlerType, GetLifetime(scope)));
        }

        return this;
    }
}
