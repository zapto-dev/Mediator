using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddNotificationHandler<TNotification, THandler>(
        this IServiceCollection services,
        MediatorNamespace? ns = null)
        where TNotification : INotification
        where THandler : class, INotificationHandler<TNotification>
    {
        if (ns == null)
        {
            services.AddScoped<INotificationHandler<TNotification>, THandler>();
        }
        else
        {
            services.TryAddScoped<THandler>();
            services.AddScoped<INamespaceNotificationHandler<TNotification>>(p => new NamespaceNotificationHandlerProvider<TNotification, THandler>(ns.Value, p));
        }

        return services;
    }

    public static IServiceCollection AddNotificationHandler<TNotification>(
        this IServiceCollection services,
        INotificationHandler<TNotification> handler,
        MediatorNamespace? ns = null)
        where TNotification : INotification
    {
        if (ns == null)
        {
            services.AddSingleton(handler);
        }
        else
        {
            services.AddSingleton<INamespaceNotificationHandler<TNotification>>(
                new NamespaceNotificationHandler<TNotification>(ns.Value, handler)
            );
        }

        return services;
    }

    public static IServiceCollection AddNotificationHandler<TNotification>(
        this IServiceCollection services,
        Func<TNotification, ValueTask> handler,
        MediatorNamespace? ns = null)
        where TNotification : INotification
    {
        AddNotificationHandler(services, new FuncNotificationHandler<TNotification>(handler), ns);
        return services;
    }

    public static IServiceCollection AddNotificationHandler<TNotification>(
        this IServiceCollection services,
        Action<TNotification> handler,
        MediatorNamespace? ns = null)
        where TNotification : INotification
    {
        AddNotificationHandler<TNotification>(services, n =>
        {
            handler(n);
            return default;
        }, ns);
        return services;
    }
}
