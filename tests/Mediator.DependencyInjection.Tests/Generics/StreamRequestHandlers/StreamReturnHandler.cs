using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class ReturnStreamHandler : IStreamRequestHandler<ReturnStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(IServiceProvider provider, ReturnStreamRequest request,
        CancellationToken cancellationToken)
    {
        yield return request.Value;
    }
}
