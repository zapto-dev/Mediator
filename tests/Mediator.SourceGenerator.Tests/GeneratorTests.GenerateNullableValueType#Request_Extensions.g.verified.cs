//HintName: Request_Extensions.g.cs
#nullable enable

public static partial class SenderExtensions
{
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::System.Nullable<int>> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, global::System.Nullable<int>>(request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::System.Nullable<int>> RequestAsync(this global::Zapto.Mediator.ISender sender, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, global::System.Nullable<int>>(new global::Request(), cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::System.Nullable<int>> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, global::System.Nullable<int>>(ns, request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::System.Nullable<int>> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, global::System.Nullable<int>>(ns, new global::Request(), cancellationToken);
}
