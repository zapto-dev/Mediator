﻿using System;
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
        Func<IServiceProvider, TRequest, ValueTask<TResponse>> handler,
        MediatorNamespace? ns = null)
        where TRequest : IRequest<TResponse>
    {
        if (ns == null)
        {
            services.AddScoped<IRequestHandler<TRequest, TResponse>>(p =>
                new FuncRequestHandler<TRequest, TResponse>(handler, p));
        }
        else
        {
            services.AddSingleton<INamespaceRequestHandler<TRequest, TResponse>>(p =>
                new NamespaceRequestHandler<TRequest, TResponse>(ns.Value,
                    new FuncRequestHandler<TRequest, TResponse>(handler, p)));
        }

        return services;
    }

    public static IServiceCollection AddRequestHandler(
        this IServiceCollection services,
        Delegate handler,
        MediatorNamespace? ns = null)
    {
        RegisterHandler(
            registerMethodName: nameof(AddRequestHandler),
            parameterTypeTarget: typeof(IRequest<>),
            noResultMessage: "No request found in delegate",
            multipleResultMessage: "Multiple requests found in delegate",
            services: services,
            handler: handler,
            ns: ns);

        return services;
    }
}
