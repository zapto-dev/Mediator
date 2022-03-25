using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            services.AddTransient<INotificationHandler<TNotification>, THandler>();
        }
        else
        {
            services.TryAddTransient<THandler>();
            services.AddSingleton<INamespaceNotificationHandler<TNotification>>(p => new NamespaceNotificationHandlerProvider<TNotification, THandler>(ns.Value, p));
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
        Func<IServiceProvider, TNotification, ValueTask> handler,
        MediatorNamespace? ns = null)
        where TNotification : INotification
    {
        if (ns == null)
        {
            services.AddSingleton<INotificationHandler<TNotification>>(p =>
                new FuncNotificationHandler<TNotification>(handler, p));
        }
        else
        {
            services.AddSingleton<INamespaceNotificationHandler<TNotification>>(p =>
                new NamespaceNotificationHandler<TNotification>(ns.Value,
                    new FuncNotificationHandler<TNotification>(handler, p)));
        }

        return services;
    }

    public static IServiceCollection AddNotificationHandler(
        this IServiceCollection services,
        Delegate handler,
        MediatorNamespace? ns = null)
    {
        RegisterHandler(
            registerMethodName: nameof(AddNotificationHandler),
            parameterTypeTarget: typeof(INotification),
            noResultMessage: "No notification found in delegate",
            multipleResultMessage: "Multiple notifications found in delegate",
            services: services,
            handler: handler,
            ns: ns);

        return services;
    }
}
