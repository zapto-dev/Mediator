//HintName: MediatorExtensions.g.cs
#nullable enable

namespace Zapto.Mediator
{
    internal static class AssemblyExtensions_Tests
    {
        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddAssemblyHandlers(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            services.AddRequestHandler(typeof(global::RequestHandler));
            return services;
        }
    }
}

public static class TestsSenderExtensions
{
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request, global::MediatR.Unit>(request, cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request, global::MediatR.Unit>(new global::Request(), cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request, global::MediatR.Unit>(ns, request, cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request, global::MediatR.Unit>(ns, new global::Request(), cancellationToken);
}
