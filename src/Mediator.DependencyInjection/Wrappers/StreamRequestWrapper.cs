#pragma warning disable CS8425

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediatR;

namespace Zapto.Mediator.Wrappers;

internal interface IStreamRequestWrapper
{
    IAsyncEnumerable<object?> Handle(object streamRequest, CancellationToken cancellationToken, IMediator mediator);

    IAsyncEnumerable<object?> Handle(MediatorNamespace ns, object streamRequest, CancellationToken cancellationToken, IMediator mediator);
}

internal interface IStreamRequestWrapper<out TResponse> : IStreamRequestWrapper
{
    new IAsyncEnumerable<TResponse> Handle(object streamRequest, CancellationToken cancellationToken, IMediator mediator);

    new IAsyncEnumerable<TResponse> Handle(MediatorNamespace ns, object streamRequest, CancellationToken cancellationToken, IMediator mediator);
}

internal static class StreamRequestWrapper
{
    private static readonly ConcurrentDictionary<Type, IStreamRequestWrapper> StreamRequestHandlers = new();

    public static IStreamRequestWrapper Get(Type type)
    {
        return StreamRequestHandlers.GetOrAdd(type, static t =>
        {
            var interfaceType = t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequest<>));

            if (interfaceType == null)
            {
                throw new InvalidOperationException($"Type {t.Name} is not a StreamRequest");
            }

            var responseType = interfaceType.GetGenericArguments().First();

            return (IStreamRequestWrapper)Activator.CreateInstance(typeof(StreamRequestWrapper<,>).MakeGenericType(t, responseType));
        });
    }

    public static IStreamRequestWrapper<TResponse> Get<TResponse>(Type type)
    {
        return (IStreamRequestWrapper<TResponse>)Get(type);
    }
}

internal sealed class StreamRequestWrapper<TRequest, TResponse> : IStreamRequestWrapper<TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public IAsyncEnumerable<TResponse> Handle(object streamRequest, CancellationToken cancellationToken, IMediator mediator)
    {
        return mediator.CreateStream<TRequest, TResponse>((TRequest)streamRequest, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> Handle(MediatorNamespace ns, object streamRequest, CancellationToken cancellationToken, IMediator mediator)
    {
        return mediator.CreateStream<TRequest, TResponse>(ns, (TRequest)streamRequest, cancellationToken);
    }

    async IAsyncEnumerable<object?> IStreamRequestWrapper.Handle(MediatorNamespace ns, object streamRequest, CancellationToken cancellationToken, IMediator mediator)
    {
        await foreach(var response in Handle(ns, (TRequest)streamRequest, cancellationToken, mediator))
        {
            yield return response;
        }
    }

    async IAsyncEnumerable<object?> IStreamRequestWrapper.Handle(object streamRequest, CancellationToken cancellationToken, IMediator mediator)
    {
        await foreach(var response in Handle((TRequest)streamRequest, cancellationToken, mediator))
        {
            yield return response;
        }
    }
}
