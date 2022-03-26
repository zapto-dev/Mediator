using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddRequestHandler(
        this IServiceCollection services,
        Type requestType,
        Type? responseType,
        Type handlerType)
    {
        if (requestType.IsGenericType || responseType is null || responseType.IsGenericType)
        {
            services.AddTransient(handlerType);
            services.AddSingleton(new GenericRequestRegistration(requestType, responseType, handlerType));
        }
        else
        {
            services.AddTransient(typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType), handlerType);
        }

        return services;
    }

    public static IServiceCollection AddRequestHandler(this IServiceCollection services, Type handlerType)
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

            services.AddRequestHandler(
                requestType,
                responseType switch
                {
                    { IsGenericParameter: true } => null,
                    { IsGenericType: true } => responseType.GetGenericTypeDefinition(),
                    _ => responseType
                },
                handlerType);
        }

        return services;
    }

    public static IServiceCollection AddRequestHandler<THandler>(this IServiceCollection services)
        where THandler : IRequestHandler
    {
        AddRequestHandler(services, typeof(THandler));
        return services;
    }
}
