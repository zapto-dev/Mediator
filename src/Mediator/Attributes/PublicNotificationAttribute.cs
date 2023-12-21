using System;

namespace Zapto.Mediator;

/// <summary>
/// Mark the notification as public.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PublicNotificationAttribute : Attribute;
