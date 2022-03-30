using Benchmarks.Models;
using Zapto.Mediator;

namespace Benchmarks.Handlers.Zapto;

public class ReturnStructGenericHandlerZapto<T> : IRequestHandler<ReturnStructGeneric<T>, T>
{
    public ValueTask<T> Handle(IServiceProvider provider, ReturnStructGeneric<T> request, CancellationToken cancellationToken)
        => new(request.Value);
}
