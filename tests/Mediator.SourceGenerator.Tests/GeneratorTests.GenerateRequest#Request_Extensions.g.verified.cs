//HintName: Request_Extensions.g.cs
#nullable enable

public static partial class SenderExtensions
{
    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, global::MediatR.Unit>(request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, string Argument, int OptionalArgument = 0, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, global::MediatR.Unit>(new global::Request(Argument, OptionalArgument), cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request request, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, global::MediatR.Unit>(ns, request, cancellationToken);

    [global::System.Diagnostics.DebuggerStepThrough]
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, string Argument, int OptionalArgument = 0, global::System.Threading.CancellationToken cancellationToken = default)
    => sender.Send<global::Request, global::MediatR.Unit>(ns, new global::Request(Argument, OptionalArgument), cancellationToken);
}
