using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator.Wrappers;

internal interface IRequestWrapper
{
    ValueTask Handle(object request, CancellationToken cancellationToken, IMediator mediator);

    ValueTask Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator);
}

internal interface IRequestResponseWrapper
{
    ValueTask<object?> Handle(object request, CancellationToken cancellationToken, IMediator mediator);

    ValueTask<object?> Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator);
}

internal interface IRequestResponseWrapper<TResponse> : IRequestResponseWrapper
{
    new ValueTask<TResponse> Handle(object request, CancellationToken cancellationToken, IMediator mediator);

    new ValueTask<TResponse> Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator);
}

internal static class RequestWrapper
{
    private static readonly ConcurrentDictionary<Type, IRequestWrapper> RequestHandlers = new();
    private static readonly ConcurrentDictionary<Type, IRequestResponseWrapper> RequestWithResponseHandlers = new();

    public static IRequestWrapper GetWithoutResponse(Type type)
    {
        return RequestHandlers.GetOrAdd(type, static t =>
        {
            if (!typeof(IRequest).IsAssignableFrom(t))
            {
                throw new InvalidOperationException($"Type {t.Name} is not a request");
            }

            return (IRequestWrapper)Activator.CreateInstance(typeof(RequestWrapper<>).MakeGenericType(t));
        });
    }

    public static IRequestResponseWrapper GetWithResponse(Type type)
    {
        return RequestWithResponseHandlers.GetOrAdd(type, static t =>
        {
            var interfaceType = t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

            if (interfaceType == null)
            {
                throw new InvalidOperationException($"Type {t.Name} is not a request");
            }

            var responseType = interfaceType.GetGenericArguments().First();

            return (IRequestResponseWrapper)Activator.CreateInstance(typeof(RequestResponseWrapper<,>).MakeGenericType(t, responseType));
        });
    }

    public static IRequestResponseWrapper<TResponse> GetWithResponse<TResponse>(Type type)
    {
        return (IRequestResponseWrapper<TResponse>)GetWithResponse(type);
    }
}

internal sealed class RequestWrapper<TRequest> : IRequestWrapper
    where TRequest : IRequest
{
    public ValueTask Handle(object request, CancellationToken cancellationToken, IMediator mediator)
    {
        return mediator.Send((TRequest)request, cancellationToken);
    }

    public ValueTask Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator)
    {
        return mediator.Send(ns, (TRequest)request, cancellationToken);
    }
}

internal sealed class RequestResponseWrapper<TRequest, TResponse> : IRequestResponseWrapper<TResponse>
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

    async ValueTask<object?> IRequestResponseWrapper.Handle(MediatorNamespace ns, object request, CancellationToken cancellationToken, IMediator mediator)
    {
        return await Handle(ns, request, cancellationToken, mediator);
    }

    async ValueTask<object?> IRequestResponseWrapper.Handle(object request, CancellationToken cancellationToken, IMediator mediator)
    {
        return await Handle(request, cancellationToken, mediator);
    }
}
