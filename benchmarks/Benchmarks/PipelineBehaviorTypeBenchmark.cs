using BenchmarkDotNet.Attributes;
using Benchmarks.Handlers.Zapto;
using Benchmarks.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[MemoryDiagnoser]
public class PipelineBehaviorTypeBenchmark
{
    private readonly MediatR.IMediator _mediatR;
    private readonly Zapto.Mediator.IMediator _mediator;

    public PipelineBehaviorTypeBenchmark()
    {
        var services = new ServiceCollection();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Benchmarks.Handlers.MediatR.AddExclamationMarkPipelineBehavior<,>));

        Zapto.Mediator.ServiceExtensions.AddMediator(services, builder =>
        {
            builder.AddRequestHandler<Ping, string, PingHandlerZapto>();
            builder.AddPipelineBehavior(typeof(Benchmarks.Handlers.Zapto.AddExclamationMarkPipelineBehavior<Ping>));
        });

        var provider = services.BuildServiceProvider();

        _mediatR = provider.GetRequiredService<MediatR.IMediator>();
        _mediator = provider.GetRequiredService<Zapto.Mediator.IMediator>();
    }

    [Benchmark]
    public async ValueTask<string> MediatR_Interface() => await _mediatR.Send(new Ping());

    [Benchmark]
    public async ValueTask<string?> MediatR_Object() => (string?) await _mediatR.Send((object) new Ping());

    [Benchmark]
    public async ValueTask<string> Zapto_Generic() => await _mediator.PingAsync();

    [Benchmark]
    public async ValueTask<string> Zapto_Interface() => await _mediator.Send(new Ping());

    [Benchmark]
    public async ValueTask<string?> Zapto_Object() => (string?) await _mediator.Send((object) new Ping());
}
