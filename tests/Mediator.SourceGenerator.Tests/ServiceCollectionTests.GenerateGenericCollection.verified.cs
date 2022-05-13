//HintName: MediatorExtensions.g.cs
#nullable enable

namespace Zapto.Mediator
{
    internal static class AssemblyExtensions_Tests
    {
        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddAssemblyHandlers(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            services.AddRequestHandler(typeof(global::RequestHandler<>));
            return services;
        }
    }
}

public static class TestsSenderExtensions
{
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Request<T> request, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, global::MediatR.Unit>(request, cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, global::MediatR.Unit>(new global::Request<T>(), cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request<T> request, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, global::MediatR.Unit>(ns, request, cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, global::MediatR.Unit>(ns, new global::Request<T>(), cancellationToken);
}
