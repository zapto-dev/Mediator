﻿using MediatR;
using Zapto.Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var upperNs = new MediatorNamespace("upper");

services.AddMediator(builder =>
{
    // Add a request handler for "GetMessage"
    builder.AddRequestHandler((GetMessage _) => "Hello world");

    // Add a notification handler for "WriteLineNotification"
    builder.AddNotificationHandler((WriteLineNotification notification) =>
    {
        Console.WriteLine(notification.Message);
    });
});

services.AddMediator(upperNs, builder =>
{
    // Add a notification handler for "WriteLineNotification" with-in the mediator namespace "upper"
    builder.AddNotificationHandler((WriteLineNotification notification) =>
    {
        Console.WriteLine(notification.Message.ToUpper());
    });
});

// Create the service provider and execute the request and notifications
// Note that the extension methods 'GetMessageAsync' and 'WriteLineAsync' are generated by the source generator
await using var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();
var message = await mediator.GetMessageAsync();

await mediator.WriteLineAsync(message);
await mediator.WriteLineAsync(upperNs, message);

public record struct WriteLineNotification(string Message) : INotification;

public record struct GetMessage : IRequest<string>;
