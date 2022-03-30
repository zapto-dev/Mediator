using System;
using MediatR;

namespace Zapto.Mediator;

internal class NamespaceNotificationHandler<TNotification> : INamespaceNotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly INotificationHandler<TNotification> _handler;

    public NamespaceNotificationHandler(MediatorNamespace ns, INotificationHandler<TNotification> handler)
    {
        Namespace = ns;
        _handler = handler;
    }

    public MediatorNamespace Namespace { get; }

    public INotificationHandler<TNotification> GetHandler(IServiceProvider provider)
    {
        return _handler;
    }
}