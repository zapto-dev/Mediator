using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddRequestHandler(
        this IServiceCollection services,
        Type requestType,
        Type responseType,
        Type handlerType)
    {
        services.AddScoped(handlerType);
        services.AddSingleton(new GenericRegistration(requestType, responseType, handlerType));
        return services;
    }

    public static IServiceCollection AddRequestHandler(
        this IServiceCollection services,
        Type requestType,
        Type handlerType)
    {
        AddRequestHandler(services, requestType, typeof(Unit), handlerType);
        return services;
    }
}
