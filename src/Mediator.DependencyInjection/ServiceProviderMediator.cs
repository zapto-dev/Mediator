using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zapto.Mediator.Wrappers;

namespace Zapto.Mediator;

public class ServiceProviderMediator : PublisherBase, IMediator
{
    private IBackgroundPublisher? _background;
    private readonly IServiceProvider _provider;

    public IBackgroundPublisher Background => _background ??= _provider.GetRequiredService<IBackgroundPublisher>();

    public ServiceProviderMediator(IServiceProvider provider) : base(provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public ValueTask Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.GetWithoutResponse(request.GetType());

        return handler.Handle(request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.GetWithResponse<TResponse>(request.GetType());

        return handler.Handle(request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask Send(MediatorNamespace ns, IRequest request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.GetWithoutResponse(request.GetType());

        return handler.Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Send<TResponse>(MediatorNamespace ns, IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.GetWithResponse<TResponse>(request.GetType());

        return handler.Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.GetWithResponse(request.GetType());

        return handler.Handle(request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask<object?> Send(MediatorNamespace ns, object request, CancellationToken cancellationToken = default)
    {
        var handler = RequestWrapper.GetWithResponse(request.GetType());

        return handler.Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
    public ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        var handler = _provider.GetRequiredService<IRequestHandler<TRequest>>();
        var pipeline = _provider.GetServices<IPipelineBehavior<TRequest, Unit>>();

        if (pipeline is IPipelineBehavior<TRequest, Unit>[] array)
        {
            return SendWithPipelineArray(request, array, handler, cancellationToken);
        }

        return SendWithPipelineEnumerable(request, handler, pipeline, cancellationToken);
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
    public async ValueTask Send<TRequest>(MediatorNamespace ns, TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        var namespaceHandler = _provider
            .GetServices<INamespaceRequestHandler<TRequest>>()
            .FirstOrDefault(i => i.Namespace == ns);

        if (namespaceHandler == null)
        {
            throw new NamespaceHandlerNotFoundException();
        }

        var pipeline = _provider.GetServices<IPipelineBehavior<TRequest, Unit>>();
        var handler = namespaceHandler.GetHandler(_provider);

        if (pipeline is IPipelineBehavior<TRequest, Unit>[] array)
        {
            await SendWithPipelineArray(request, array, handler, cancellationToken);
        }
        else
        {
            await SendWithPipelineEnumerable(request, handler, pipeline, cancellationToken);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Unit> Wrap(ValueTask task)
    {
        return task.IsCompletedSuccessfully
            ? new ValueTask<Unit>(Unit.Value)
            : Awaited(task);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static async ValueTask<Unit> Awaited(ValueTask task)
        {
            await task;
            return Unit.Value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask Unwrap(ValueTask<Unit> task)
    {
        return task.IsCompletedSuccessfully
            ? default
            : Awaited(task);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static async ValueTask Awaited(ValueTask<Unit> task)
        {
            await task;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask SendWithPipelineArray<TRequest>(
        TRequest request,
        IPipelineBehavior<TRequest, Unit>[] array,
        IRequestHandler<TRequest> handler,
        CancellationToken cancellationToken
    ) where TRequest : IRequest
    {
        var generic = _provider.GetRequiredService<GenericPipelineBehavior<TRequest, Unit>>();

        if (array.Length == 0 && generic.IsEmpty)
        {
            return handler.Handle(_provider, request, cancellationToken);
        }

        return SendWithPipelineArraySlow(request, array, handler, cancellationToken, generic);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ValueTask SendWithPipelineArraySlow<TRequest>(
        TRequest request,
        IPipelineBehavior<TRequest, Unit>[] array,
        IRequestHandler<TRequest> handler,
        CancellationToken cancellationToken,
        GenericPipelineBehavior<TRequest, Unit> generic
    ) where TRequest : IRequest
    {
        RequestHandlerDelegate<Unit> next = () => Wrap(handler.Handle(_provider, request, cancellationToken));

        for (var i = array.Length - 1; i >= 0; i--)
        {
            var pipelineBehavior = array[i];
            var nextPipeline = next;
            next = () => pipelineBehavior.Handle(_provider, request, nextPipeline, cancellationToken);
        }

        return Unwrap(generic.Handle(_provider, request, next, cancellationToken));
    }

    private ValueTask SendWithPipelineEnumerable<TRequest>(
        TRequest request,
        IRequestHandler<TRequest> handler,
        IEnumerable<IPipelineBehavior<TRequest, Unit>> pipeline,
        CancellationToken cancellationToken
    ) where TRequest : IRequest
    {
        var generic = _provider.GetRequiredService<GenericPipelineBehavior<TRequest, Unit>>();

        RequestHandlerDelegate<Unit> next = () => Wrap(handler.Handle(_provider, request, cancellationToken));

        foreach (var pipelineBehavior in pipeline.Reverse())
        {
            var nextPipeline = next;
            next = () => pipelineBehavior.Handle(_provider, request, nextPipeline, cancellationToken);
        }

        return Unwrap(generic.Handle(_provider, request, next, cancellationToken));
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<TResponse> SendWithPipelineArray<TRequest, TResponse>(
        TRequest request,
        IPipelineBehavior<TRequest, TResponse>[] array,
        IRequestHandler<TRequest, TResponse> handler,
        CancellationToken cancellationToken
    ) where TRequest : IRequest<TResponse>
    {
        var generic = _provider.GetRequiredService<GenericPipelineBehavior<TRequest, TResponse>>();

        if (array.Length == 0 && generic.IsEmpty)
        {
            return handler.Handle(_provider, request, cancellationToken);
        }

        return SendWithPipelineArraySlow(request, array, handler, cancellationToken, generic);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ValueTask<TResponse> SendWithPipelineArraySlow<TRequest, TResponse>(
        TRequest request,
        IPipelineBehavior<TRequest, TResponse>[] array,
        IRequestHandler<TRequest, TResponse> handler,
        CancellationToken cancellationToken,
        GenericPipelineBehavior<TRequest, TResponse> generic
    ) where TRequest : IRequest<TResponse>
    {
        RequestHandlerDelegate<TResponse> next = () => handler.Handle(_provider, request, cancellationToken);

        for (var i = array.Length - 1; i >= 0; i--)
        {
            var pipelineBehavior = array[i];
            var nextPipeline = next;
            next = () => pipelineBehavior.Handle(_provider, request, nextPipeline, cancellationToken);
        }

        return generic.Handle(_provider, request, next, cancellationToken);
    }

    private ValueTask<TResponse> SendWithPipelineEnumerable<TRequest, TResponse>(
        TRequest request,
        IRequestHandler<TRequest, TResponse> handler,
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipeline,
        CancellationToken cancellationToken
    ) where TRequest : IRequest<TResponse>
    {
        var generic = _provider.GetRequiredService<GenericPipelineBehavior<TRequest, TResponse>>();

        RequestHandlerDelegate<TResponse> next = () => handler.Handle(_provider, request, cancellationToken);

        foreach (var pipelineBehavior in pipeline.Reverse())
        {
            var nextPipeline = next;
            next = () => pipelineBehavior.Handle(_provider, request, nextPipeline, cancellationToken);
        }

        return generic.Handle(_provider, request, next, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(MediatorNamespace ns, IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return StreamRequestWrapper.Get<TResponse>(request.GetType()).Handle(ns, request, cancellationToken, this);
    }

    /// <inheritdoc />
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
}
