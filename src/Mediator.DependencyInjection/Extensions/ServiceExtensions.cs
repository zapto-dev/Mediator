using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IMediatorBuilder AddMediator(this IServiceCollection services)
    {
        var builder = new MediatorBuilder(services);

        services.TryAddTransient<IBackgroundPublisher, DefaultBackgroundPublisher>();

        services.TryAddTransient<IMediator, ServiceProviderMediator>();
        services.TryAddTransient<ISender, ServiceProviderMediator>();
        services.TryAddTransient<IPublisher, ServiceProviderMediator>();

        services.TryAddSingleton(typeof(GenericRequestCache<,>));
        services.TryAddTransient(typeof(IRequestHandler<,>), typeof(GenericRequestHandler<,>));

        services.TryAddSingleton(typeof(GenericNotificationCache<>));
        services.TryAddTransient(typeof(GenericNotificationHandler<>));

        services.TryAddSingleton(typeof(GenericPipelineBehavior<,>));

        services.TryAddSingleton(typeof(GenericStreamRequestCache<,>));
        services.TryAddTransient(typeof(IStreamRequestHandler<,>), typeof(GenericStreamRequestHandler<,>));

        return builder;
    }

    public static IServiceCollection AddMediator(this IServiceCollection services, Action<IMediatorBuilder> configure)
    {
        configure(services.AddMediator());
        return services;
    }

    public static IMediatorBuilder AddMediator(this IServiceCollection services, string ns)
    {
        return AddMediator(services).AddNamespace(ns);
    }

    public static IServiceCollection AddMediator(this IServiceCollection services, string ns, Action<IMediatorBuilder> configure)
    {
        configure(services.AddMediator(ns));
        return services;
    }

    public static IMediatorBuilder AddMediator(this IServiceCollection services, MediatorNamespace ns)
    {
        return AddMediator(services).AddNamespace(ns);
    }

    public static IServiceCollection AddMediator(this IServiceCollection services, MediatorNamespace ns, Action<IMediatorBuilder> configure)
    {
        configure(services.AddMediator(ns));
        return services;
    }
}
