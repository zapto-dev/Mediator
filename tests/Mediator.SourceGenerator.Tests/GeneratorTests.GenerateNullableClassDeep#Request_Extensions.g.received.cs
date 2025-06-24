//HintName: Request_Extensions.g.cs
#nullable enable

public static partial class SenderExtensions
{
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<string?> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, string?>(request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<string?> RequestAsync(this global::Zapto.Mediator.ISender sender, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, string?>(new global::Request(), cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<string?> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, string?>(ns, request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<string?> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, string?>(ns, new global::Request(), cancellationToken);
}
