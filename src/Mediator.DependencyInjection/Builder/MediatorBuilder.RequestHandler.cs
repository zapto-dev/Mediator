using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddRequestHandler<TRequest, TResponse, THandler>(RegistrationScope scope = RegistrationScope.Transient)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        if (_ns == null)
        {
            _services.Add(new ServiceDescriptor(typeof(IRequestHandler<TRequest, TResponse>), typeof(THandler), GetLifetime(scope)));
        }
        else
        {
            _services.TryAdd(new ServiceDescriptor(typeof(THandler), typeof(THandler), GetLifetime(scope)));
            _services.AddSingleton<INamespaceRequestHandler<TRequest, TResponse>>(new NamespaceRequestHandlerProvider<TRequest, TResponse, THandler>(_ns.Value));
        }

        return this;
    }

    public IMediatorBuilder AddRequestHandler<TRequest, THandler>(RegistrationScope scope = RegistrationScope.Transient)
        where TRequest : IRequest<Unit>
        where THandler : class, IRequestHandler<TRequest, Unit>
    {
        return AddRequestHandler<TRequest, Unit, THandler>(scope);
    }

    public IMediatorBuilder AddRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        if (_ns == null)
        {
            _services.AddSingleton(handler);
        }
        else
        {
            _services.AddSingleton<INamespaceRequestHandler<TRequest, TResponse>>(
                new NamespaceRequestHandler<TRequest, TResponse>(_ns.Value, handler)
            );
        }

        return this;
    }

    public IMediatorBuilder AddRequestHandler<TRequest, TResponse>(Func<IServiceProvider, TRequest, ValueTask<TResponse>> handler)
        where TRequest : IRequest<TResponse>
    {
        if (_ns == null)
        {
            _services.AddSingleton<IRequestHandler<TRequest, TResponse>>(
                new FuncRequestHandler<TRequest, TResponse>(handler));
        }
        else
        {
            _services.AddSingleton<INamespaceRequestHandler<TRequest, TResponse>>(
                new NamespaceRequestHandler<TRequest, TResponse>(_ns.Value,
                    new FuncRequestHandler<TRequest, TResponse>(handler)));
        }

        return this;
    }

    public IMediatorBuilder AddRequestHandler(Delegate handler)
    {
        RegisterHandler(
            registerMethodName: nameof(AddRequestHandler),
            parameterTypeTarget: typeof(IRequest<>),
            noResultMessage: "No request found in delegate",
            multipleResultMessage: "Multiple requests found in delegate",
            handler: handler);

        return this;
    }
}
