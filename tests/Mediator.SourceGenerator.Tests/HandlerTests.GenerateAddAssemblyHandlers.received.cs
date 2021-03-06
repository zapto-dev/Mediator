//HintName: MediatorExtensions.g.cs
#nullable enable

public static class TestsSenderExtensions
{
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request, global::MediatR.Unit>(new global::Request(), cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit> RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request, global::MediatR.Unit>(ns, new global::Request(), cancellationToken);
}
