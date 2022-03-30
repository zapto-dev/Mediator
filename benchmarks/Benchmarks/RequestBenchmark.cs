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
    private readonly PingHandlerZapto _handler;
    private readonly Zapto.Mediator.IRequestHandler<Ping, string> _handlerInterface;
    private readonly ServiceProvider _provider;

    public RequestBenchmark()
    {
        var services = new ServiceCollection();

        services.AddMediatR(typeof(Program).Assembly);

        services.AddMediator();
        services.AddRequestHandler<Ping, string, PingHandlerZapto>();

        var provider = services.BuildServiceProvider();

        _provider = provider;
        _handler = new PingHandlerZapto();
        _handlerInterface = new PingHandlerZapto();
        _mediatR = provider.GetRequiredService<MediatR.IMediator>();
        _mediator = provider.GetRequiredService<Zapto.Mediator.IMediator>();
    }

    [Benchmark(Baseline = true)]
    public ValueTask<string> Handler_AsInterface() => _handlerInterface.Handle(_provider, new Ping(), CancellationToken.None);

    [Benchmark]
    public ValueTask<string> Handler_AsClass() => _handler.Handle(_provider, new Ping(), CancellationToken.None);

    [Benchmark]
    public Task<string> MediatR_Interface() => _mediatR.Send(new Ping());

    [Benchmark]
    public Task<object?> MediatR_Object() => _mediatR.Send((object) new Ping());

    [Benchmark]
    public ValueTask<string> Zapto_Generic() => _mediator.PingAsync();

    [Benchmark]
    public ValueTask<string> Zapto_Interface() => _mediator.Send(new Ping());

    [Benchmark]
    public ValueTask<object?> Zapto_Object() => _mediator.Send((object) new Ping());
}
