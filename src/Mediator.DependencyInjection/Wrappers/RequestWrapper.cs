using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator.Wrappers;

internal interface IRequestWrapper
{
    ValueTask<object?> Handle(object request, CancellationToken cancellationToken, IMediator mediator);

    ValueTask<object?> Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator);
}

internal interface IRequestWrapper<TResponse> : IRequestWrapper
{
    new ValueTask<TResponse> Handle(object request, CancellationToken cancellationToken, IMediator mediator);

    new ValueTask<TResponse> Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator);
}

internal static class RequestWrapper
{
    private static readonly ConcurrentDictionary<Type, IRequestWrapper> RequestHandlers = new();

    public static IRequestWrapper Get(Type type)
    {
        return RequestHandlers.GetOrAdd(type, t =>
        {
            var interfaceType = t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

            if (interfaceType == null)
            {
                throw new InvalidOperationException($"Type {t.Name} is not a request");
            }

            var responseType = interfaceType.GetGenericArguments().First();

            return (IRequestWrapper)Activator.CreateInstance(typeof(RequestWrapper<,>).MakeGenericType(type, responseType));
        });
    }

    public static IRequestWrapper<TResponse> Get<TResponse>(Type type)
    {
        return (IRequestWrapper<TResponse>)Get(type);
    }
}

internal sealed class RequestWrapper<TRequest, TResponse> : IRequestWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public ValueTask<TResponse> Handle(object request, CancellationToken cancellationToken, IMediator mediator)
    {
        return mediator.Send<TRequest, TResponse>((TRequest)request, cancellationToken);
    }

    public ValueTask<TResponse> Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator)
    {
        return mediator.Send<TRequest, TResponse>(ns, (TRequest)request, cancellationToken);
    }

    async ValueTask<object?> IRequestWrapper.Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator)
    {
        return await Handle(ns, request, cancellationToken, mediator);
    }

    async ValueTask<object?> IRequestWrapper.Handle(object request, CancellationToken cancellationToken, IMediator mediator)
    {
        return await Handle(request, cancellationToken, mediator);
    }
}
