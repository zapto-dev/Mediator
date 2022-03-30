using BenchmarkDotNet.Attributes;
using Benchmarks.Handlers.Zapto;
using Benchmarks.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zapto.Mediator;

namespace Benchmarks;

[MemoryDiagnoser]
public class StructRequestBenchmark
{
    private static readonly MediatorNamespace Namespace = new("Custom");
    private readonly MediatR.IMediator _mediatR;
    private readonly Zapto.Mediator.IMediator _mediator;
    private readonly PingStructHandlerZapto _handler;
    private readonly Zapto.Mediator.IRequestHandler<PingStruct, string> _handlerInterface;
    private readonly ServiceProvider _provider;

    public StructRequestBenchmark()
    {
        var services = new ServiceCollection();

        services.AddMediatR(typeof(Program).Assembly);

        services.AddMediator();
        services.AddRequestHandler<PingStruct, string, PingStructHandlerZapto>();

        var provider = services.BuildServiceProvider();

        _provider = provider;
        _handler = new PingStructHandlerZapto();
        _handlerInterface = new PingStructHandlerZapto();
        _mediatR = provider.GetRequiredService<MediatR.IMediator>();
        _mediator = provider.GetRequiredService<Zapto.Mediator.IMediator>();
    }

    [Benchmark(Baseline = true)]
    public ValueTask<string> Handler_AsInterface() => _handlerInterface.Handle(_provider, new PingStruct(), CancellationToken.None);

    [Benchmark]
    public ValueTask<string> Handler_AsClass() => _handler.Handle(_provider, new PingStruct(), CancellationToken.None);

    [Benchmark]
    public Task<string> MediatR_Interface() => _mediatR.Send(new PingStruct());

    [Benchmark]
    public Task<object?> MediatR_Object() => _mediatR.Send((object) new PingStruct());

    [Benchmark]
    public ValueTask<string> Zapto_Generic() => _mediator.PingStructAsync();

    [Benchmark]
    public ValueTask<string> Zapto_Interface() => _mediator.Send(new PingStruct());

    [Benchmark]
    public ValueTask<object?> Zapto_Object() => _mediator.Send((object) new PingStruct());
}
