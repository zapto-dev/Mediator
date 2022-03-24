using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal class NamespaceNotificationHandlerProvider<TNotification, THandler> : INamespaceNotificationHandler<TNotification>
    where TNotification : INotification
    where THandler : INotificationHandler<TNotification>
{
    private readonly IServiceProvider _serviceProvider;

    public NamespaceNotificationHandlerProvider(MediatorNamespace ns, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Namespace = ns;
    }

    public MediatorNamespace Namespace { get; }

    public THandler Handler => _serviceProvider.GetRequiredService<THandler>();

    INotificationHandler<TNotification> INamespaceNotificationHandler<TNotification>.Handler => Handler;
}
