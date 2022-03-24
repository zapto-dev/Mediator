using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public interface INamespaceNotificationHandler<in TNotification> : INamespaceHandler
    where TNotification : INotification
{
    INotificationHandler<TNotification> Handler { get; }
}
