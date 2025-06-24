//HintName: Request_Extensions.g.cs
#nullable enable

public static partial class SenderExtensions
{
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask RequestAsync(this global::Zapto.Mediator.ISender sender, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request>(request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask RequestAsync(this global::Zapto.Mediator.ISender sender, string RequiredProperty, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request>(new global::Request() { RequiredProperty = RequiredProperty }, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request>(ns, request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, string RequiredProperty, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request>(ns, new global::Request() { RequiredProperty = RequiredProperty }, cancellationToken);
}
