using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class ReturnNullableGenericStreamHandler<TType> : IStreamRequestHandler<ReturnNullableGenericStreamRequest<TType>, TType?>
    where TType : struct
{
    public ValueTask<TType?> Handle(IServiceProvider provider, ReturnNullableGenericRequest<TType> request,
        CancellationToken cancellationToken)
    {
        return new ValueTask<TType?>(request.Value);
    }

    public async IAsyncEnumerable<TType?> Handle(IServiceProvider provider, ReturnNullableGenericStreamRequest<TType> request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        yield return request.Value;
    }
}
