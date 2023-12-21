using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

public static class MediatorBuilderExtensions
{
    public static IMediatorBuilder AddNamespace(this IMediatorBuilder builder, string ns)
    {
        return builder.AddNamespace(new MediatorNamespace(ns));
    }

    public static IMediatorBuilder AddNamespace(this IMediatorBuilder builder, string ns, Action<IMediatorBuilder> configure)
    {
        configure(builder.AddNamespace(ns));
        return builder;
    }

    public static IMediatorBuilder AddNamespace(this IMediatorBuilder builder, MediatorNamespace ns, Action<IMediatorBuilder> configure)
    {
        configure(builder.AddNamespace(ns));
        return builder;
    }

    public static IMediatorBuilder AddPipelineBehavior<TBehavior>(this IMediatorBuilder builder)
    {
        return builder.AddPipelineBehavior(typeof(TBehavior));
    }

    public static IMediatorBuilder AddStreamPipelineBehavior<TBehavior>(this IMediatorBuilder builder)
    {
        return builder.AddStreamPipelineBehavior(typeof(TBehavior));
    }

    public static IMediatorBuilder AddPipelineBehavior<TRequest, TResponse>(this IMediatorBuilder builder, RequestMiddleware<TRequest, TResponse> middleware)
        where TRequest : notnull
    {
        return builder.AddPipelineBehavior(new DelegatePipelineBehavior<TRequest, TResponse>(middleware));
    }

    public static IMediatorBuilder AddStreamPipelineBehavior<TRequest, TResponse>(this IMediatorBuilder builder, StreamMiddleware<TRequest, TResponse> middleware)
        where TRequest : IStreamRequest<TResponse>
    {
        return builder.AddStreamPipelineBehavior(new DelegateStreamPipelineBehavior<TRequest, TResponse>(middleware));
    }
}

public interface IMediatorBuilder
{
    IMediatorBuilder AddNamespace(MediatorNamespace ns);

    IMediatorBuilder AddRequestHandler<THandler>() where THandler : IRequestHandler;

    IMediatorBuilder AddRequestHandler(Type handlerType);

    IMediatorBuilder AddRequestHandler(Type requestType, Type? responseType, Type handlerType);

    IMediatorBuilder AddRequestHandler<TRequest, TResponse, THandler>()
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>;

    IMediatorBuilder AddRequestHandler<TRequest, THandler>()
        where TRequest : IRequest<Unit>
        where THandler : class, IRequestHandler<TRequest, Unit>;

    IMediatorBuilder AddRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>;

    IMediatorBuilder AddRequestHandler<TRequest, TResponse>(Func<IServiceProvider, TRequest, ValueTask<TResponse>> handler)
        where TRequest : IRequest<TResponse>;

    IMediatorBuilder AddRequestHandler(Delegate handler);

    IMediatorBuilder AddNotificationHandler(Type notificationType, Type handlerType);

    IMediatorBuilder AddNotificationHandler(Type handlerType);

    IMediatorBuilder AddNotificationHandler<THandler>()
        where THandler : INotificationHandler;

    IMediatorBuilder AddNotificationHandler<TNotification, THandler>()
        where TNotification : INotification
        where THandler : class, INotificationHandler<TNotification>;

    IMediatorBuilder AddNotificationHandler<TNotification>(INotificationHandler<TNotification> handler)
        where TNotification : INotification;

    IMediatorBuilder AddNotificationHandler<TNotification>(Func<IServiceProvider, TNotification, ValueTask> handler)
        where TNotification : INotification;

    IMediatorBuilder AddNotificationHandler(Delegate handler);

    IMediatorBuilder AddStreamRequestHandler(Type type);

    IMediatorBuilder AddStreamRequestHandler<THandler>() where THandler : IStreamRequestHandler;

    IMediatorBuilder AddStreamRequestHandler(Type requestType, Type? responseType, Type handlerType);

    IMediatorBuilder AddStreamRequestHandler<TRequest, TResponse, THandler>()
        where TRequest : IStreamRequest<TResponse>
        where THandler : class, IStreamRequestHandler<TRequest, TResponse>;

    IMediatorBuilder AddStreamRequestHandler<TRequest, THandler>()
        where TRequest : IStreamRequest<Unit>
        where THandler : class, IStreamRequestHandler<TRequest, Unit>;

    IMediatorBuilder AddStreamRequestHandler<TRequest, TResponse>(IStreamRequestHandler<TRequest, TResponse> handler)
        where TRequest : IStreamRequest<TResponse>;

    public IMediatorBuilder AddStreamRequestHandler<TRequest, TResponse>(Func<IServiceProvider, TRequest, IAsyncEnumerable<TResponse>> handler)
        where TRequest : IStreamRequest<TResponse>;

    IMediatorBuilder AddDefaultRequestHandler(Type handlerType);

    IMediatorBuilder AddDefaultRequestHandler<THandler>() where THandler : class, IDefaultRequestHandler;

    IMediatorBuilder AddDefaultNotificationHandler(Type handlerType);

    IMediatorBuilder AddDefaultNotificationHandler<THandler>() where THandler : class, IDefaultNotificationHandler;

    IMediatorBuilder AddDefaultStreamRequestHandler(Type handlerType);

    IMediatorBuilder AddDefaultStreamRequestHandler<THandler>() where THandler : class, IDefaultStreamRequestHandler;

    IMediatorBuilder AddPipelineBehavior<TRequest, TResponse>(IPipelineBehavior<TRequest, TResponse> behavior)
        where TRequest : notnull;

    IMediatorBuilder AddPipelineBehavior(Type behaviorType);

    IMediatorBuilder AddPipelineBehavior(Type requestType, Type? responseType, Type behaviorType);

    IMediatorBuilder AddPipelineBehavior<TRequest, TResponse, TBehavior>()
        where TRequest : notnull
        where TBehavior : class, IPipelineBehavior<TRequest, TResponse>;

    IMediatorBuilder AddStreamPipelineBehavior<TRequest, TResponse>(IStreamPipelineBehavior<TRequest, TResponse> behavior)
        where TRequest : IStreamRequest<TResponse>;

    IMediatorBuilder AddStreamPipelineBehavior(Type behaviorType);

    IMediatorBuilder AddStreamPipelineBehavior(Type requestType, Type? responseType, Type behaviorType);

    IMediatorBuilder AddStreamPipelineBehavior<TRequest, TResponse, TBehavior>()
        where TRequest : IStreamRequest<TResponse>
        where TBehavior : class, IStreamPipelineBehavior<TRequest, TResponse>;
}
