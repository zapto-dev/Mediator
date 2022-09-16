using System;

namespace Zapto.Mediator;

/// <summary>
/// Register the method to be called when a notification is sent.
/// The class should be registered with <see cref="IPublisher.RegisterNotificationHandler"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NotificationHandlerAttribute : Attribute
{
    
}
