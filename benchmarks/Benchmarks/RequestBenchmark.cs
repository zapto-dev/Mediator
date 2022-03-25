using BenchmarkDotNet.Attributes;
using Benchmarks.Handlers.Zapto;
using Benchmarks.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zapto.Mediator;

namespace Benchmarks;

[MemoryDiagnoser]
public class RequestBenchmark
{
    private static readonly MediatorNamespace Namespace = new("Custom");
    private readonly MediatR.IMediator _mediatR;
    private readonly Zapto.Mediator.IMediator _mediator;

    public RequestBenchmark()
    {
        var services = new ServiceCollection();

        services.AddMediatR(typeof(Program).Assembly);

        services.AddMediator();
        services.AddRequestHandler<Ping, string, PingHandlerZapto>();
        services.AddRequestHandler((PingDelegate _) => "pong");
        services.AddRequestHandler<Ping, string, PingHandlerZapto>(Namespace);
        services.AddRequestHandler(typeof(ReturnGenericHandlerZapto<>));

        var provider = services.BuildServiceProvider();

        _mediatR = provider.GetRequiredService<MediatR.IMediator>();
        _mediator = provider.GetRequiredService<Zapto.Mediator.IMediator>();
    }

    [Benchmark]
    public async ValueTask<string> MediatR() => await _mediatR.Send(new Ping());

    [Benchmark]
    public async ValueTask<string> Zapto() => await _mediator.PingAsync();

    [Benchmark]
    public async ValueTask<string> ZaptoDelegate() => await _mediator.PingDelegateAsync();

    [Benchmark]
    public async ValueTask<string> ZaptoNamespace() => await _mediator.PingAsync(Namespace);

    [Benchmark]
    public async ValueTask<string> ZaptoGeneric() => await _mediator.ReturnGenericAsync("pong");
}
