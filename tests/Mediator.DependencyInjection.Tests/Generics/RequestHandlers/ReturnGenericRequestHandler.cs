using System;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class ReturnGenericRequestHandler<TValue> : IRequestHandler<ReturnGenericRequest<TValue>, TValue>
{
    public ValueTask<TValue> Handle(IServiceProvider provider, ReturnGenericRequest<TValue> request,
        CancellationToken cancellationToken)
    {
        return new ValueTask<TValue>(request.Value);
    }
}
