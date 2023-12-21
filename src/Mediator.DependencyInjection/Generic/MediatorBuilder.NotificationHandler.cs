using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    public IMediatorBuilder AddNotificationHandler(
        Type notificationType,
        Type handlerType,
        RegistrationScope scope = RegistrationScope.Transient)
    {
        if (notificationType.IsGenericType)
        {
            _services.Add(new ServiceDescriptor(handlerType, handlerType, GetLifetime(scope)));
            _services.AddSingleton(new GenericNotificationRegistration(notificationType, handlerType));
        }
        else
        {
            _services.Add(new ServiceDescriptor(typeof(INotificationHandler<>).MakeGenericType(notificationType), handlerType, GetLifetime(scope)));
        }

        return this;
    }

    public IMediatorBuilder AddNotificationHandler(Type handlerType, RegistrationScope scope = RegistrationScope.Transient)
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
                handlerType,
                scope);
        }

        return this;
    }

    public IMediatorBuilder AddNotificationHandler<THandler>(RegistrationScope scope = RegistrationScope.Transient)
        where THandler : INotificationHandler
    {
        AddNotificationHandler(typeof(THandler), scope);
        return this;
    }
}
