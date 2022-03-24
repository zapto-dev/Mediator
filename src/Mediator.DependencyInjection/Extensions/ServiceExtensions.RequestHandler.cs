using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddRequestHandler<TRequest, TResponse, THandler>(
        this IServiceCollection services,
        MediatorNamespace? ns = null)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        if (ns == null)
        {
            services.AddScoped<IRequestHandler<TRequest, TResponse>, THandler>();
        }
        else
        {
            services.TryAddScoped<THandler>();
            services.AddScoped<INamespaceRequestHandler<TRequest, TResponse>>(p => new NamespaceRequestHandlerProvider<TRequest, TResponse, THandler>(ns.Value, p));
        }

        return services;
    }

    public static IServiceCollection AddRequestHandler<TRequest, THandler>(
        this IServiceCollection services,
        MediatorNamespace? ns = null)
        where TRequest : IRequest<Unit>
        where THandler : class, IRequestHandler<TRequest, Unit>
    {
        return AddRequestHandler<TRequest, Unit, THandler>(services, ns);
    }

    public static IServiceCollection AddRequestHandler<TRequest, TResponse>(
        this IServiceCollection services,
        IRequestHandler<TRequest, TResponse> handler,
        MediatorNamespace? ns = null)
        where TRequest : IRequest<TResponse>
    {
        if (ns == null)
        {
            services.AddSingleton(handler);
        }
        else
        {
            services.AddSingleton<INamespaceRequestHandler<TRequest, TResponse>>(
                new NamespaceRequestHandler<TRequest, TResponse>(ns.Value, handler)
            );
        }

        return services;
    }

    public static IServiceCollection AddRequestHandler<TRequest, TResponse>(
        this IServiceCollection services,
        Func<TRequest, ValueTask<TResponse>> handler,
        MediatorNamespace? ns = null)
        where TRequest : IRequest<TResponse>
    {
        AddRequestHandler(services, new FuncRequestHandler<TRequest, TResponse>(handler), ns);
        return services;
    }

    public static IServiceCollection AddRequestHandler<TRequest, TResponse>(
        this IServiceCollection services,
        Func<TRequest, TResponse> handler,
        MediatorNamespace? ns = null)
        where TRequest : IRequest<TResponse>
    {
        AddRequestHandler<TRequest, TResponse>(services, r => new ValueTask<TResponse>(handler(r)), ns);
        return services;
    }
}
