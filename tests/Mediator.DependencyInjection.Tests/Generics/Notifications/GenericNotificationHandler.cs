using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class GenericNotificationHandler<T> : INotificationHandler<GenericNotification<T>>
{
    private readonly Result _result;

    public GenericNotificationHandler(Result result)
    {
        _result = result;
    }

    public ValueTask Handle(GenericNotification<T> notification, CancellationToken cancellationToken)
    {
        _result.Object = notification.Value;
        return default;
    }
}