//HintName: MediatorExtensions.g.cs
#nullable enable

namespace Zapto.Mediator
{
    internal static class AssemblyExtensions_Tests
    {
        public static global::Zapto.Mediator.IMediatorBuilder AddAssemblyHandlers(this global::Zapto.Mediator.IMediatorBuilder builder)
        {
            builder.AddRequestHandler(typeof(global::RequestHandler<>));
            return builder;
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
