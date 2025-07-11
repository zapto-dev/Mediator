﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddNotificationHandler<TNotification, THandler>(RegistrationScope scope = RegistrationScope.Transient)
        where TNotification : INotification
        where THandler : class, INotificationHandler<TNotification>
    {
        if (_ns == null)
        {
            _services.Add(new ServiceDescriptor(typeof(INotificationHandler<TNotification>), typeof(THandler), GetLifetime(scope)));
        }
        else
        {
            _services.TryAdd(new ServiceDescriptor(typeof(THandler), typeof(THandler), GetLifetime(scope)));
            _services.AddSingleton<INamespaceNotificationHandler<TNotification>>(new NamespaceNotificationHandlerProvider<TNotification, THandler>(_ns.Value));
        }

        return this;
    }

    public IMediatorBuilder AddNotificationHandler<TNotification>(INotificationHandler<TNotification> handler)
        where TNotification : INotification
    {
        if (_ns == null)
        {
            _services.AddSingleton(handler);
        }
        else
        {
            _services.AddSingleton<INamespaceNotificationHandler<TNotification>>(
                new NamespaceNotificationHandler<TNotification>(_ns.Value, handler)
            );
        }

        return this;
    }

    public IMediatorBuilder AddNotificationHandler<TNotification>(Func<IServiceProvider, TNotification, CancellationToken, ValueTask> handler)
        where TNotification : INotification
    {
        if (_ns == null)
        {
            _services.AddSingleton<INotificationHandler<TNotification>>(
                new FuncNotificationHandler<TNotification>(handler));
        }
        else
        {
            _services.AddSingleton<INamespaceNotificationHandler<TNotification>>(
                new NamespaceNotificationHandler<TNotification>(_ns.Value,
                    new FuncNotificationHandler<TNotification>(handler)));
        }

        return this;
    }

    private static readonly Type[] NotificationParameterTypeTargets = new[]
    {
        typeof(INotification),
    };

    public IMediatorBuilder AddNotificationHandler(Delegate handler)
    {
        RegisterHandler(
            registerMethodName: nameof(AddNotificationHandler),
            parameterTypeTargets: NotificationParameterTypeTargets,
            noResultMessage: "No notification found in delegate",
            multipleResultMessage: "Multiple notifications found in delegate",
            handler: handler);

        return this;
    }
}
