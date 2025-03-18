//HintName: Request_Extensions.g.cs
#nullable enable

public static partial class SenderExtensions
{
    /// <summary>
    /// Sends a request to the handler <see cref="global::RequestHandler"/>.
    /// </summary>
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask RequestAsync(this global::Zapto.Mediator.ISender sender, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request>(request, cancellationToken);

    /// <summary>
    /// Sends a request to the handler <see cref="global::RequestHandler"/>.
    /// </summary>
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask RequestAsync(this global::Zapto.Mediator.ISender sender, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request>(new global::Request(), cancellationToken);

    /// <summary>
    /// Sends a request to the handler <see cref="global::RequestHandler"/>.
    /// </summary>
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request>(ns, request, cancellationToken);

    /// <summary>
    /// Sends a request to the handler <see cref="global::RequestHandler"/>.
    /// </summary>
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request>(ns, new global::Request(), cancellationToken);
}
