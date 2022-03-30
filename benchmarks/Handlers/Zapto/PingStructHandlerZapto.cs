using Benchmarks.Models;
using Zapto.Mediator;

namespace Benchmarks.Handlers.Zapto;

public sealed class PingStructHandlerZapto : IRequestHandler<PingStruct, string>
{
    public ValueTask<string> Handle(IServiceProvider provider, PingStruct request, CancellationToken cancellationToken)
    {
        return new ValueTask<string>("Pong");
    }
}
