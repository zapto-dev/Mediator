﻿using Benchmarks.Models;
using Zapto.Mediator;

namespace Benchmarks.Handlers.Zapto;

public class PingHandlerZapto : IRequestHandler<Ping, string>
{
    public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<string>("Pong");
    }
}