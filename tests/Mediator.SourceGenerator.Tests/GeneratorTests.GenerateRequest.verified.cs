//HintName: RequestExtensions.g.cs
#nullable enable

public static class TestsSenderExtensions
{
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit?>? RequestAsync(this global::Zapto.Mediator.ISender sender, string Argument, int OptionalArgument = 0, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request, global::MediatR.Unit>(new global::Request(Argument, OptionalArgument), cancellationToken);
    public static global::System.Threading.Tasks.ValueTask<global::MediatR.Unit?>? RequestAsync(this global::Zapto.Mediator.ISender sender, global::Zapto.Mediator.MediatorNamespace ns, string Argument, int OptionalArgument = 0, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Send<global::Request, global::MediatR.Unit>(ns, new global::Request(Argument, OptionalArgument), cancellationToken);
}
