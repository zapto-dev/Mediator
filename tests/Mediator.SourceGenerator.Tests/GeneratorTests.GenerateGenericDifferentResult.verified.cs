//HintName: MediatorExtensions.g.cs
#nullable enable

public static class TestsSenderExtensions
{
    public static global::System.Threading.Tasks.ValueTask<string> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Request<T> request, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, string>(request, cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<string> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, T Argument, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, string>(new global::Request<T>(Argument), cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<string> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, global::Request<T> request, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, string>(ns, request, cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<string> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, T Argument, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, string>(ns, new global::Request<T>(Argument), cancellationToken);
}
