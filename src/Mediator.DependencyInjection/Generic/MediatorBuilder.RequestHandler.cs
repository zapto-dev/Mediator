﻿using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddRequestHandler(
        Type requestType,
        Type? responseType,
        Type handlerType,
        RegistrationScope scope = RegistrationScope.Transient)
    {
        if (requestType.IsGenericType || responseType is null || responseType.IsGenericTypeDefinition)
        {
            _services.Add(new ServiceDescriptor(handlerType, handlerType, GetLifetime(scope)));
            _services.AddSingleton(new GenericRequestRegistration(requestType, responseType, handlerType));
        }
        else
        {
            _services.Add(new ServiceDescriptor(typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType), handlerType, GetLifetime(scope)));
        }

        return this;
    }

    public IMediatorBuilder AddRequestHandler(Type handlerType, RegistrationScope scope = RegistrationScope.Transient)
    {
        var handlers = handlerType.GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .ToArray();

        foreach (var type in handlers)
        {
            var requestType = type.GetGenericArguments()[0];
            var responseType = type.GetGenericArguments()[1];

            if (requestType.IsGenericType)
            {
                requestType = requestType.GetGenericTypeDefinition();
            }

            AddRequestHandler(
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

    private static bool HasGenericParameter(Type type)
    {
        if (type.IsGenericParameter)
        {
            return true;
        }

        if (type.IsGenericType)
        {
            return type.GetGenericArguments().Any(HasGenericParameter);
        }

        return false;
    }

    public IMediatorBuilder AddRequestHandler<THandler>(RegistrationScope scope = RegistrationScope.Transient)
        where THandler : IRequestHandler
    {
        AddRequestHandler(typeof(THandler), scope);
        return this;
    }
}
