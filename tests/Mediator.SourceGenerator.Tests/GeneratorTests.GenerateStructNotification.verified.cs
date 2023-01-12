//HintName: MediatorExtensions.g.cs
#nullable enable

public static class TestsSenderExtensions
{
    [global::System.Diagnostics.DebuggerStepThrough] public static global::System.Threading.Tasks.ValueTask NotificationAsync(this global::Zapto.Mediator.IPublisher sender, global::Notification notification, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Publish<global::Notification>(notification, cancellationToken);
    [global::System.Diagnostics.DebuggerStepThrough] public static global::System.Threading.Tasks.ValueTask NotificationAsync(this global::Zapto.Mediator.IPublisher sender, string Argument, int OptionalArgument = 0, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Publish<global::Notification>(new global::Notification(Argument, OptionalArgument), cancellationToken);
    [global::System.Diagnostics.DebuggerStepThrough] public static global::System.Threading.Tasks.ValueTask NotificationAsync(this global::Zapto.Mediator.IPublisher sender, global::Zapto.Mediator.MediatorNamespace ns, global::Notification notification, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Publish<global::Notification>(ns, notification, cancellationToken);
    [global::System.Diagnostics.DebuggerStepThrough] public static global::System.Threading.Tasks.ValueTask NotificationAsync(this global::Zapto.Mediator.IPublisher sender, global::Zapto.Mediator.MediatorNamespace ns, string Argument, int OptionalArgument = 0, global::System.Threading.CancellationToken cancellationToken = default)
        => sender.Publish<global::Notification>(ns, new global::Notification(Argument, OptionalArgument), cancellationToken);
}
