using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal static class NotificationAttributeHandler<T>
{
    static NotificationAttributeHandler()
    {
        foreach (var method in typeof(T).GetMethods())
        {
            var attribute = method.GetCustomAttribute<NotificationHandlerAttribute>();

            if (attribute == null) continue;

            var notificationType = method.GetParameters()[0].ParameterType;

            if (!typeof(INotification).IsAssignableFrom(notificationType))
            {
                throw new InvalidOperationException("Invalid notification type");
            }

            var cacheType = typeof(GenericNotificationCache<>).MakeGenericType(notificationType);

            Handlers.Add((cacheType, CreateHandler(method, notificationType)));
        }
    }

    public static List<(Type, Func<T, IServiceProvider, object, CancellationToken, ValueTask>)> Handlers { get; } = new();

    public static IDisposable RegisterHandlers(IServiceProvider serviceProvider, object obj, Func<Func<Task>, Task>? middleware)
    {
        var registrations = new List<IHandlerRegistration>();
        var handler = (T)obj;

        foreach (var (cacheType, invoker) in Handlers)
        {
            var cache = (INotificationCache) serviceProvider.GetRequiredService(cacheType);
            var registration = new HandlerRegistration(cache, invoker, handler, middleware);

            cache.Lock.Wait();
            cache.Registrations.Add(registration);
            cache.Lock.Release();

            registrations.Add(registration);
        }

        return new RemoveTemporaryHandler(registrations);
    }

    private sealed class RemoveTemporaryHandler : IDisposable
    {
        private readonly List<IHandlerRegistration> _registrations;

        public RemoveTemporaryHandler(List<IHandlerRegistration> registrations)
        {
            _registrations = registrations;
        }

        public void Dispose()
        {
            foreach (var registration in _registrations)
            {
                var owner = registration.Owner;

                owner.Lock.Wait();
                owner.Registrations.Remove(registration);
                owner.Lock.Release();
            }
        }
    }

    private class HandlerRegistration : IHandlerRegistration
    {
        private readonly T _handler;
        private readonly Func<Func<Task>, Task>? _middleware;

        public HandlerRegistration(
            INotificationCache owner,
            Func<T, IServiceProvider, object, CancellationToken, ValueTask> invoke,
            T handler,
            Func<Func<Task>, Task>? middleware)
        {
            Invoke = invoke;
            _handler = handler;
            _middleware = middleware;
            Owner = owner;
        }

        public INotificationCache Owner { get; }

        public Func<T, IServiceProvider, object, CancellationToken, ValueTask> Invoke { get; }

        public bool IsDisposed { get; set; }

        public ValueTask InvokeAsync(IServiceProvider provider, object notification, CancellationToken cancellationToken)
        {
            if (IsDisposed)
            {
                return default;
            }

            if (_middleware != null)
            {
                return new ValueTask(
                    _middleware(() => !IsDisposed ? Invoke(_handler, provider, notification, cancellationToken).AsTask() : Task.CompletedTask)
                );
            }

            return Invoke(_handler, provider, notification, cancellationToken);
        }
    }

    private static Func<T, IServiceProvider, object, CancellationToken, ValueTask> CreateHandler(MethodInfo method, Type notificationType)
    {
        var handler = Expression.Parameter(typeof(T), "handler");
        var provider = Expression.Parameter(typeof(IServiceProvider), "provider");
        var notification = Expression.Parameter(typeof(object), "notification");
        var cancellationToken = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var parameters = method.GetParameters();
        var arguments = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.ParameterType == typeof(IServiceProvider))
            {
                arguments[i] = provider;
            }
            else if (parameter.ParameterType == notificationType)
            {
                arguments[i] = Expression.Convert(notification, notificationType);
            }
            else if (parameter.ParameterType == typeof(CancellationToken))
            {
                arguments[i] = cancellationToken;
            }
            else
            {
                arguments[i] = Expression.Convert(Expression.Call(provider, "GetRequiredService", new[] {parameter.ParameterType}), parameter.ParameterType);
            }
        }

        Expression handlerExpression = Expression.Call(
            handler,
            method,
            arguments);

        if (handlerExpression.Type == typeof(void))
        {
            handlerExpression = Expression.Block(handlerExpression, Expression.Default(typeof(ValueTask)));
        }
        else if (handlerExpression.Type == typeof(Task))
        {
            handlerExpression = Expression.New(
                typeof(ValueTask).GetConstructor(new[] {typeof(Task)})!,
                handlerExpression);
        }

        return Expression.Lambda<Func<T, IServiceProvider, object, CancellationToken, ValueTask>>(
            handlerExpression,
            handler,
            provider,
            notification,
            cancellationToken
        ).Compile();
    }
}
