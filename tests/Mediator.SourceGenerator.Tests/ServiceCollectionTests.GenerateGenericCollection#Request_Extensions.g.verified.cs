﻿//HintName: Request_Extensions.g.cs
#nullable enable

public static partial class SenderExtensions
{
    /// <summary>
    /// Sends a request to the handler <see cref="global::RequestHandler<T>"/>.
    /// </summary>
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Request<T> request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request<T>, global::MediatR.Unit>(request, cancellationToken);

    /// <summary>
    /// Sends a request to the handler <see cref="global::RequestHandler<T>"/>.
    /// </summary>
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request<T>, global::MediatR.Unit>(new global::Request<T>(), cancellationToken);

    /// <summary>
    /// Sends a request to the handler <see cref="global::RequestHandler<T>"/>.
    /// </summary>
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request<T> request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request<T>, global::MediatR.Unit>(ns, request, cancellationToken);

    /// <summary>
    /// Sends a request to the handler <see cref="global::RequestHandler<T>"/>.
    /// </summary>
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request<T>, global::MediatR.Unit>(ns, new global::Request<T>(), cancellationToken);
}
