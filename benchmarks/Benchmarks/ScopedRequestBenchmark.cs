using BenchmarkDotNet.Attributes;
using Benchmarks.Handlers.Zapto;
using Benchmarks.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zapto.Mediator;

namespace Benchmarks;

[MemoryDiagnoser]
public class ScopedRequestBenchmark
{
    private static readonly MediatorNamespace Namespace = new("Custom");
    private readonly ServiceProvider _provider;

    public ScopedRequestBenchmark()
    {
        var services = new ServiceCollection();

        services.AddMediatR(typeof(Program).Assembly);

        services.AddMediator();
        services.AddRequestHandler<Ping, string, PingHandlerZapto>();
        services.AddRequestHandler((PingDelegate _) => "pong");
        services.AddRequestHandler<Ping, string, PingHandlerZapto>(Namespace);

        _provider = services.BuildServiceProvider();
    }

    [Benchmark]
    public async ValueTask<string> MediatR()
    {
        await using var scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<MediatR.IMediator>().Send(new Ping());
    }

    [Benchmark]
    public async ValueTask<string> Zapto()
    {
        await using var scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<Zapto.Mediator.IMediator>().PingAsync();
    }

    [Benchmark]
    public async ValueTask<string> ZaptoDelegate()
    {
        await using var scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<Zapto.Mediator.IMediator>().PingDelegateAsync();
    }

    [Benchmark]
    public async ValueTask<string> ZaptoNamespace()
    {
        await using var scope = _provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<Zapto.Mediator.IMediator>().PingAsync(Namespace);
    }
}
