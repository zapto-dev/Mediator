﻿//HintName: MediatorExtensions.g.cs
#nullable enable

public static class TestsSenderExtensions
{
    public static global::System.Threading.Tasks.ValueTask<T> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, T Argument, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, T>(new global::Request<T>(Argument), cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<T> RequestAsync<T>(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, T Argument, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request<T>, T>(ns, new global::Request<T>(Argument), cancellationToken);
}
