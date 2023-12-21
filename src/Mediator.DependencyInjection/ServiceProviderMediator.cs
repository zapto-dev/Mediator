using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zapto.Mediator.Wrappers;

namespace Zapto.Mediator;

public class ServiceProviderMediator : IMediator
{
    private readonly IServiceProvider _provider;

    public ServiceProviderMediator(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.Get<TResponse>(request.GetType());

        return handler.Handle(request, cancellationToken, this);
    }

    public ValueTask<TResponse> Send<TResponse>(MediatorNamespace ns, IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.Get<TResponse>(request.GetType());

        return handler.Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.Get(request.GetType());

        return handler.Handle(request, cancellationToken, this);
    }

    public ValueTask<object?> Send(MediatorNamespace ns, object request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.Get(request.GetType());

        return handler.Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
    {
        var handler = _provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var pipeline = _provider.GetServices<IPipelineBehavior<TRequest, TResponse>>();

        return pipeline is IPipelineBehavior<TRequest, TResponse>[] array
            ? SendWithPipelineArray(request, array, handler, cancellationToken)
            : SendWithPipelineEnumerable(request, handler, pipeline, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Send<TRequest, TResponse>(MediatorNamespace ns, TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        var namespaceHandler = _provider
            .GetServices<INamespaceRequestHandler<TRequest, TResponse>>()
            .FirstOrDefault(i => i.Namespace == ns);

        if (namespaceHandler == null)
        {
            throw new NamespaceHandlerNotFoundException();
        }

        var pipeline = _provider.GetServices<IPipelineBehavior<TRequest, TResponse>>();
        var handler = namespaceHandler.GetHandler(_provider);

        return pipeline is IPipelineBehavior<TRequest, TResponse>[] array
            ? await SendWithPipelineArray(request, array, handler, cancellationToken)
            : await SendWithPipelineEnumerable(request, handler, pipeline, cancellationToken);
    }

    private ValueTask<TResponse> SendWithPipelineArray<TRequest, TResponse>(
        TRequest request,
        IPipelineBehavior<TRequest, TResponse>[] array,
        IRequestHandler<TRequest, TResponse> handler,
        CancellationToken cancellationToken
    ) where TRequest : IRequest<TResponse>
    {
        if (array.Length == 0)
        {
            return handler.Handle(_provider, request, cancellationToken);
        }

        RequestHandlerDelegate<TResponse> next = () => handler.Handle(_provider, request, cancellationToken);

        for (var i = array.Length - 1; i >= 0; i--)
        {
            var pipelineBehavior = array[i];
            var nextPipeline = next;
            next = () => pipelineBehavior.Handle(_provider, request, nextPipeline, cancellationToken);
        }

        return next();
    }

    private ValueTask<TResponse> SendWithPipelineEnumerable<TRequest, TResponse>(
        TRequest request,
        IRequestHandler<TRequest, TResponse> handler,
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipeline,
        CancellationToken cancellationToken
    ) where TRequest : IRequest<TResponse>
    {
        RequestHandlerDelegate<TResponse> next = () => handler.Handle(_provider, request, cancellationToken);

        foreach (var pipelineBehavior in pipeline.Reverse())
        {
            var nextPipeline = next;
            next = () => pipelineBehavior.Handle(_provider, request, nextPipeline, cancellationToken);
        }

        return next();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(MediatorNamespace ns, IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get<TResponse>(request.GetType()).Handle(ns, request, cancellationToken, this);
    }

    public IAsyncEnumerable<object?> CreateStream(MediatorNamespace ns, object request, CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get(request.GetType()).Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get<TResponse>(request.GetType()).Handle(request, cancellationToken, this);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get(request.GetType()).Handle(request, cancellationToken, this);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>
    {
        var handler = _provider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();
        var pipeline = _provider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();

        return pipeline is IStreamPipelineBehavior<TRequest, TResponse>[] array
            ? CreateStreamWithPipelineArray(request, array, handler, cancellationToken)
            : CreateStreamWithPipelineEnumerable(request, handler, pipeline, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TRequest, TResponse>(MediatorNamespace ns, TRequest request,
        CancellationToken cancellationToken = default) where TRequest : IStreamRequest<TResponse>
    {
        var namespaceHandler = _provider
            .GetServices<INamespaceStreamRequestHandler<TRequest, TResponse>>()
            .FirstOrDefault(i => i.Namespace == ns);

        if (namespaceHandler == null)
        {
            throw new NamespaceHandlerNotFoundException();
        }

        var handler = namespaceHandler.GetHandler(_provider);
        var pipeline = _provider.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>();

        return pipeline is IStreamPipelineBehavior<TRequest, TResponse>[] array
            ? CreateStreamWithPipelineArray(request, array, handler, cancellationToken)
            : CreateStreamWithPipelineEnumerable(request, handler, pipeline, cancellationToken);
    }

    private IAsyncEnumerable<TResponse> CreateStreamWithPipelineArray<TRequest, TResponse>(
        TRequest request,
        IStreamPipelineBehavior<TRequest, TResponse>[] array,
        IStreamRequestHandler<TRequest, TResponse> handler,
        CancellationToken cancellationToken
    ) where TRequest : IStreamRequest<TResponse>
    {
        if (array.Length == 0)
        {
            return handler.Handle(_provider, request, cancellationToken);
        }

        StreamHandlerDelegate<TResponse> next = () => handler.Handle(_provider, request, cancellationToken);

        for (var i = array.Length - 1; i >= 0; i--)
        {
            var pipelineBehavior = array[i];
            var nextPipeline = next;
            next = () => pipelineBehavior.Handle(_provider, request, nextPipeline, cancellationToken);
        }

        return next();
    }

    private IAsyncEnumerable<TResponse> CreateStreamWithPipelineEnumerable<TRequest, TResponse>(
        TRequest request,
        IStreamRequestHandler<TRequest, TResponse> handler,
        IEnumerable<IStreamPipelineBehavior<TRequest, TResponse>> pipeline,
        CancellationToken cancellationToken
    ) where TRequest : IStreamRequest<TResponse>
    {
        StreamHandlerDelegate<TResponse> next = () => handler.Handle(_provider, request, cancellationToken);

        foreach (var pipelineBehavior in pipeline.Reverse())
        {
            var nextPipeline = next;
            next = () => pipelineBehavior.Handle(_provider, request, nextPipeline, cancellationToken);
        }

        return next();
    }

    /// <inheritdoc />
    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
        return NotificationWrapper.Get(notification.GetType()).Handle(notification, cancellationToken, this);
    }

    public ValueTask Publish(MediatorNamespace ns, object notification, CancellationToken cancellationToken = default)
    {
        return NotificationWrapper.Get(notification.GetType()).Handle(ns, notification, cancellationToken, this);
    }

    /// <inheritdoc />
    public async ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var didHandle = false;

        foreach (var handler in _provider.GetServices<INotificationHandler<TNotification>>())
        {
            await handler.Handle(_provider, notification, cancellationToken);
            didHandle = true;
        }

        var didGenericHandle = await _provider
            .GetRequiredService<GenericNotificationHandler<TNotification>>()
            .Handle(_provider, notification, cancellationToken);

        if (!didHandle && !didGenericHandle && _provider.GetService<IDefaultNotificationHandler>() is { } defaultHandler)
        {
            await defaultHandler.Handle(_provider, notification, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async ValueTask Publish<TNotification>(MediatorNamespace ns, TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var services = _provider
            .GetServices<INamespaceNotificationHandler<TNotification>>()
            .Where(i => i.Namespace == ns);

        foreach (var factory in services)
        {
            await factory.GetHandler(_provider).Handle(_provider, notification, cancellationToken);
        }
    }

    /// <inheritdoc />
    public IDisposable RegisterNotificationHandler(object handler, Func<Func<Task>, Task>? invokeAsync = null, bool queue = true)
    {
        if (queue)
        {
            if (invokeAsync is { } invoker)
            {
                invokeAsync = cb =>
                {
                    _ = Task.Run(() => invoker(cb));
                    return Task.CompletedTask;
                };
            }
            else
            {
                invokeAsync = Task.Run;
            }
        }

        return (IDisposable) typeof(NotificationAttributeHandler<>).MakeGenericType(handler.GetType())
            .GetMethod(nameof(NotificationAttributeHandler<object>.RegisterHandlers), BindingFlags.Static | BindingFlags.Public)!
            .Invoke(null, new[] { _provider, handler, invokeAsync });
    }
}
