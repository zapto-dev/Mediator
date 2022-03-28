using Benchmarks.Models;
using MediatR;

namespace Benchmarks.Handlers.MediatR;

public class PingStructHandlerMediatR : IRequestHandler<PingStruct, string>
{
    public Task<string> Handle(PingStruct request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }
}
