using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddNotificationHandler(
        this IServiceCollection services,
        Type notificationType,
        Type handlerType)
    {
        if (notificationType.IsGenericType)
        {
            services.AddTransient(handlerType);
            services.AddSingleton(new GenericNotificationRegistration(notificationType, handlerType));
        }
        else
        {
            services.AddTransient(typeof(INotificationHandler<>).MakeGenericType(notificationType), handlerType);
        }

        return services;
    }

    public static IServiceCollection AddNotificationHandler(this IServiceCollection services, Type handlerType)
    {
        var handlers = handlerType.GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            .ToArray();

        foreach (var type in handlers)
        {
            var notificationType = type.GetGenericArguments()[0];

            if (notificationType.IsGenericType)
            {
                notificationType = notificationType.GetGenericTypeDefinition();
            }

            services.AddNotificationHandler(
                notificationType,
                handlerType);
        }

        return services;
    }

    public static IServiceCollection AddNotificationHandler<THandler>(this IServiceCollection services)
        where THandler : INotificationHandler
    {
        AddNotificationHandler(services, typeof(THandler));
        return services;
    }
}
