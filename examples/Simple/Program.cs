using Zapto.Mediator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

var upperNs = new MediatorNamespace("upper");

await using var provider = new ServiceCollection()
    .AddMediator()
    .AddRequestHandler((GetMessage _) => "Hello world")
    .AddNotificationHandler((WriteLineNotification notification) =>
    {
        Console.WriteLine(notification.Message);
    })
    .AddNotificationHandler((WriteLineNotification notification) =>
    {
        Console.WriteLine(notification.Message.ToUpper());
    }, upperNs)
    .BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();
var message = await mediator.GetMessageAsync();

await mediator.WriteLineAsync(message);
await mediator.WriteLineAsync(upperNs, message);

public record struct WriteLineNotification(string Message) : INotification;

public record struct GetMessage : IRequest<string>;
