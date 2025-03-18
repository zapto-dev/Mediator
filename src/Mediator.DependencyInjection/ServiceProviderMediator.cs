using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
        var generic = _provider.GetRequiredService<GenericPipelineBehavior<TRequest, TResponse>>();

        if (array.Length == 0 && generic.IsEmpty)
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
}
