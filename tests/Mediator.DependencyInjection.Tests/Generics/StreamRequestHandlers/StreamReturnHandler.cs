using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class ReturnStreamHandler : IStreamRequestHandler<ReturnStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(IServiceProvider provider, ReturnStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        yield return request.Value;
    }
}
