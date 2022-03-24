using MediatR;

namespace Zapto.Mediator;

internal class NamespaceNotificationHandler<TNotification> : INamespaceNotificationHandler<TNotification>
    where TNotification : INotification
{
    public NamespaceNotificationHandler(MediatorNamespace ns, INotificationHandler<TNotification> handler)
    {
        Namespace = ns;
        Handler = handler;
    }

    public MediatorNamespace Namespace { get; }

    public INotificationHandler<TNotification> Handler { get; }
}