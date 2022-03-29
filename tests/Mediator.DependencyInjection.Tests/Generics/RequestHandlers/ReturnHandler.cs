using System;
using System.Threading;
using System.Threading.Tasks;
using Zapto.Mediator;

namespace Mediator.DependencyInjection.Tests.Generics;

public class ReturnHandler : IRequestHandler<ReturnRequest, string>
{
    public ValueTask<string> Handle(IServiceProvider provider, ReturnRequest request,
        CancellationToken cancellationToken)
    {
        return new ValueTask<string>(request.Value);
    }
}
