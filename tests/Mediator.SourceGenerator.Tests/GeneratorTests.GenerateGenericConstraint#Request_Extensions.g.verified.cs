//HintName: Request_Extensions.g.cs
#nullable enable

public static partial class SenderExtensions
{
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<T> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Request<T> request, global::System.Threading.CancellationToken cancellationToken = default)
        where T : global::MediatR.IRequest
    => sender.Send<global::Request<T>, T>(request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<T> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, T Argument, global::System.Threading.CancellationToken cancellationToken = default)
        where T : global::MediatR.IRequest
    => sender.Send<global::Request<T>, T>(new global::Request<T>(Argument), cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<T> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request<T> request, global::System.Threading.CancellationToken cancellationToken = default)
        where T : global::MediatR.IRequest
    => sender.Send<global::Request<T>, T>(ns, request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<T> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, T Argument, global::System.Threading.CancellationToken cancellationToken = default)
        where T : global::MediatR.IRequest
    => sender.Send<global::Request<T>, T>(ns, new global::Request<T>(Argument), cancellationToken);
}
