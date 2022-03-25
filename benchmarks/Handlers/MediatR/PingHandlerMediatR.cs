using Benchmarks.Models;
using MediatR;

namespace Benchmarks.Handlers.MediatR;

public class PingHandlerMediatR : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }
}
