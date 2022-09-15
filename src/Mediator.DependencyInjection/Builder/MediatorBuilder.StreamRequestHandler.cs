using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddStreamRequestHandler<TRequest, TResponse, THandler>()
        where TRequest : IStreamRequest<TResponse>
        where THandler : class, IStreamRequestHandler<TRequest, TResponse>
    {
        if (_ns == null)
        {
            _services.AddTransient<IStreamRequestHandler<TRequest, TResponse>, THandler>();
        }
        else
        {
            _services.TryAddTransient<THandler>();
            _services.AddSingleton<INamespaceStreamRequestHandler<TRequest, TResponse>>(new NamespaceStreamRequestHandlerProvider<TRequest, TResponse, THandler>(_ns.Value));
        }

        return this;
    }

    public IMediatorBuilder AddStreamRequestHandler<TRequest, THandler>()
        where TRequest : IStreamRequest<Unit>
        where THandler : class, IStreamRequestHandler<TRequest, Unit>
    {
        return AddStreamRequestHandler<TRequest, Unit, THandler>();
    }

    public IMediatorBuilder AddStreamRequestHandler<TRequest, TResponse>(IStreamRequestHandler<TRequest, TResponse> handler)
        where TRequest : IStreamRequest<TResponse>
    {
        if (_ns == null)
        {
            _services.AddSingleton(handler);
        }
        else
        {
            _services.AddSingleton<INamespaceStreamRequestHandler<TRequest, TResponse>>(
                new NamespaceStreamRequestHandler<TRequest, TResponse>(_ns.Value, handler)
            );
        }

        return this;
    }

    public IMediatorBuilder AddStreamRequestHandler<TRequest, TResponse>(Func<IServiceProvider, TRequest, IAsyncEnumerable<TResponse>> handler)
        where TRequest : IStreamRequest<TResponse>
    {
        if (_ns == null)
        {
            _services.AddSingleton<IStreamRequestHandler<TRequest, TResponse>>(
                new FuncStreamRequestHandler<TRequest, TResponse>(handler));
        }
        else
        {
            _services.AddSingleton<INamespaceStreamRequestHandler<TRequest, TResponse>>(
                new NamespaceStreamRequestHandler<TRequest, TResponse>(_ns.Value,
                    new FuncStreamRequestHandler<TRequest, TResponse>(handler)));
        }

        return this;
    }
}
