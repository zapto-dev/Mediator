using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddStreamRequestHandler<TRequest, TResponse, THandler>(
        this IServiceCollection services,
        MediatorNamespace? ns = null)
        where TRequest : IStreamRequest<TResponse>
        where THandler : class, IStreamRequestHandler<TRequest, TResponse>
    {
        if (ns == null)
        {
            services.AddTransient<IStreamRequestHandler<TRequest, TResponse>, THandler>();
        }
        else
        {
            services.TryAddTransient<THandler>();
            services.AddSingleton<INamespaceStreamRequestHandler<TRequest, TResponse>>(new NamespaceStreamRequestHandlerProvider<TRequest, TResponse, THandler>(ns.Value));
        }

        return services;
    }

    public static IServiceCollection AddStreamRequestHandler<TRequest, THandler>(
        this IServiceCollection services,
        MediatorNamespace? ns = null)
        where TRequest : IStreamRequest<Unit>
        where THandler : class, IStreamRequestHandler<TRequest, Unit>
    {
        return AddStreamRequestHandler<TRequest, Unit, THandler>(services, ns);
    }

    public static IServiceCollection AddStreamRequestHandler<TRequest, TResponse>(
        this IServiceCollection services,
        IStreamRequestHandler<TRequest, TResponse> handler,
        MediatorNamespace? ns = null)
        where TRequest : IStreamRequest<TResponse>
    {
        if (ns == null)
        {
            services.AddSingleton(handler);
        }
        else
        {
            services.AddSingleton<INamespaceStreamRequestHandler<TRequest, TResponse>>(
                new NamespaceStreamRequestHandler<TRequest, TResponse>(ns.Value, handler)
            );
        }

        return services;
    }

    public static IServiceCollection AddStreamRequestHandler<TRequest, TResponse>(
        this IServiceCollection services,
        Func<IServiceProvider, TRequest, IAsyncEnumerable<TResponse>> handler,
        MediatorNamespace? ns = null)
        where TRequest : IStreamRequest<TResponse>
    {
        if (ns == null)
        {
            services.AddSingleton<IStreamRequestHandler<TRequest, TResponse>>(
                new FuncStreamRequestHandler<TRequest, TResponse>(handler));
        }
        else
        {
            services.AddSingleton<INamespaceStreamRequestHandler<TRequest, TResponse>>(
                new NamespaceStreamRequestHandler<TRequest, TResponse>(ns.Value,
                    new FuncStreamRequestHandler<TRequest, TResponse>(handler)));
        }

        return services;
    }
}
