using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddNotificationHandler<TNotification, THandler>()
        where TNotification : INotification
        where THandler : class, INotificationHandler<TNotification>
    {
        if (_ns == null)
        {
            _services.AddTransient<INotificationHandler<TNotification>, THandler>();
        }
        else
        {
            _services.TryAddTransient<THandler>();
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

    public IMediatorBuilder AddNotificationHandler<TNotification>(Func<IServiceProvider, TNotification, ValueTask> handler)
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

    public IMediatorBuilder AddNotificationHandler(Delegate handler)
    {
        RegisterHandler(
            registerMethodName: nameof(AddNotificationHandler),
            parameterTypeTarget: typeof(INotification),
            noResultMessage: "No notification found in delegate",
            multipleResultMessage: "Multiple notifications found in delegate",
            handler: handler);

        return this;
    }
}
