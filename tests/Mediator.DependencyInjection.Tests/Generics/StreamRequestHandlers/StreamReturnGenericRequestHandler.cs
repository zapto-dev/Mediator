using System;
using System.Collections.Generic;
using System.Threading;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class ReturnGenericStreamRequestHandler<TValue> : IStreamRequestHandler<ReturnGenericStreamRequest<TValue>, TValue>
{
    public async IAsyncEnumerable<TValue> Handle(IServiceProvider provider, ReturnGenericStreamRequest<TValue> request,
        CancellationToken cancellationToken)
    {
        yield return request.Value;
    }
}
