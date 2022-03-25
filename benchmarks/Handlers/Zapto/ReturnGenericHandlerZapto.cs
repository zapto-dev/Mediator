using Benchmarks.Models;
using Zapto.Mediator;

namespace Benchmarks.Handlers.Zapto;

public class ReturnGenericHandlerZapto<T> : IRequestHandler<ReturnGeneric<T>, T>
{
    public ValueTask<T> Handle(ReturnGeneric<T> request, CancellationToken cancellationToken)
    {
        return new ValueTask<T>(request.Value);
    }
}
