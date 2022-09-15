using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddNotificationHandler(
        Type notificationType,
        Type handlerType)
    {
        if (notificationType.IsGenericType)
        {
            _services.AddTransient(handlerType);
            _services.AddSingleton(new GenericNotificationRegistration(notificationType, handlerType));
        }
        else
        {
            _services.AddTransient(typeof(INotificationHandler<>).MakeGenericType(notificationType), handlerType);
        }

        return this;
    }

    public IMediatorBuilder AddNotificationHandler(Type handlerType)
    {
        var handlers = handlerType.GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            .ToArray();

        foreach (var type in handlers)
        {
            var notificationType = type.GetGenericArguments()[0];

            if (notificationType.IsGenericType)
            {
                notificationType = notificationType.GetGenericTypeDefinition();
            }

            AddNotificationHandler(
                notificationType,
                handlerType);
        }

        return this;
    }

    public IMediatorBuilder AddNotificationHandler<THandler>()
        where THandler : INotificationHandler
    {
        AddNotificationHandler(typeof(THandler));
        return this;
    }
}
