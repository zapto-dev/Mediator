using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class ReturnGenericStreamRequestHandler<TValue> : IStreamRequestHandler<ReturnGenericStreamRequest<TValue>, TValue>
{
    public async IAsyncEnumerable<TValue> Handle(IServiceProvider provider, ReturnGenericStreamRequest<TValue> request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        yield return request.Value;
    }
}
