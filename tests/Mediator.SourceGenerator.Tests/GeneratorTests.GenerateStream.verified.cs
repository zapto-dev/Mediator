//HintName: RequestExtensions.g.cs
#nullable enable

public static class TestsSenderExtensions
{
    public static global::System.Collections.Generic.IAsyncEnumerable<int> StreamRequestAsync(this global::Zapto.Mediator.ISender sender, string Argument, int OptionalArgument = 0, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.CreateStream<global::StreamRequest, int>(new global::StreamRequest(Argument, OptionalArgument), cancellationToken);
    public static global::System.Collections.Generic.IAsyncEnumerable<int> StreamRequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, string Argument, int OptionalArgument = 0, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.CreateStream<global::StreamRequest, int>(ns, new global::StreamRequest(Argument, OptionalArgument), cancellationToken);
}
