using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

var upperNs = new MediatorNamespace("upper");

// Create provider with notification handler
await using var provider = new ServiceCollection()
    .AddMediator()
    .AddNotificationHandler((WriteLineNotification notification) =>
    {
        Console.WriteLine(notification.Message);
    })
    .AddNotificationHandler((WriteLineNotification notification) =>
    {
        Console.WriteLine(notification.Message.ToUpper());
    }, upperNs)
    .BuildServiceProvider();

// Get the mediator
var mediator = provider.GetRequiredService<IMediator>();

// Send to handlers without namespace
await mediator.WriteLineAsync("Hello World!");

// Send to the upper namespace
await mediator.WriteLineAsync(upperNs, "Hello World!");

// Notification type
public record struct WriteLineNotification(string Message) : INotification;
