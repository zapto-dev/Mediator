using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal class NamespaceNotificationHandlerProvider<TNotification, THandler> : INamespaceNotificationHandler<TNotification>
    where TNotification : INotification
    where THandler : class, INotificationHandler<TNotification>
{
    public NamespaceNotificationHandlerProvider(MediatorNamespace ns)
    {
        Namespace = ns;
    }

    public MediatorNamespace Namespace { get; }

    public INotificationHandler<TNotification> GetHandler(IServiceProvider provider)
        => provider.GetRequiredService<THandler>();
}
