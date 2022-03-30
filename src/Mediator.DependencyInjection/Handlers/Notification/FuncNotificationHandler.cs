using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

internal class FuncNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly Func<IServiceProvider, TNotification, ValueTask> _invoke;

    public FuncNotificationHandler(Func<IServiceProvider, TNotification, ValueTask> invoke)
    {
        _invoke = invoke;
    }

    public ValueTask Handle(IServiceProvider provider, TNotification notification, CancellationToken cancellationToken)
    {
        return _invoke(provider, notification);
    }
}
