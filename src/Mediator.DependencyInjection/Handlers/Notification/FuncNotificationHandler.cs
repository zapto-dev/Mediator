using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Zapto.Mediator;

internal class FuncNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, TNotification, ValueTask> _invoke;

    public FuncNotificationHandler(Func<IServiceProvider, TNotification, ValueTask> invoke, IServiceProvider serviceProvider)
    {
        _invoke = invoke;
        _serviceProvider = serviceProvider;
    }

    public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        return _invoke(_serviceProvider, notification);
    }
}
