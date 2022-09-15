using BenchmarkDotNet.Attributes;
using Benchmarks.Handlers.Zapto;
using Benchmarks.Models;
using Microsoft.Extensions.DependencyInjection;
using Zapto.Mediator;

namespace Benchmarks;

[MemoryDiagnoser]
public class StructRequestTypeBenchmark
{
    private static readonly MediatorNamespace CustomNamespace = new("Custom");
    private readonly IMediator _mediator;

    public StructRequestTypeBenchmark()
    {
        var services = new ServiceCollection();

        var builder = services.AddMediator();
        builder.AddRequestHandler<PingStruct, string, PingStructHandlerZapto>();
        builder.AddRequestHandler((PingStructDelegate _) => "pong");
        builder.AddRequestHandler(typeof(ReturnStructGenericHandlerZapto<>));

        var builderNs = builder.AddNamespace(CustomNamespace);
        builderNs.AddRequestHandler<PingStruct, string, PingStructHandlerZapto>();
        builderNs.AddRequestHandler((PingStructDelegate _) => "pong");

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Benchmark(Baseline = true)]
    public ValueTask<string> Class() => _mediator.PingStructAsync();

    [Benchmark]
    public ValueTask<string> Class_Namespace() => _mediator.PingStructAsync(CustomNamespace);

    [Benchmark]
    public ValueTask<string> Delegate() => _mediator.PingStructDelegateAsync();

    [Benchmark]
    public ValueTask<string> Delegate_Namespace() => _mediator.PingStructDelegateAsync(CustomNamespace);

    [Benchmark]
    public ValueTask<string> Generic() => _mediator.ReturnStructGenericAsync("pong");
}
