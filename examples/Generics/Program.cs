using MediatR;
using Zapto.Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Add mediator
services.AddMediator();
services.AddAssemblyHandlers(); // Add all handlers from the current assembly

// Create the service provider and execute the request
await using var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();

var typeName = await mediator.GetTypeFullNameAsync<string>();
Console.WriteLine($"The full name of string is: {typeName}");

// Requests
public record struct GetTypeFullName<T> : IRequest<string>;

// Request handlers
public class GetTypeFullNameHandler<T> : IRequestHandler<GetTypeFullName<T>, string>
{
    public ValueTask<string> Handle(IServiceProvider provider, GetTypeFullName<T> request, CancellationToken cancellationToken)
    {
        var type = typeof(T);
        return new ValueTask<string>(type.FullName ?? type.Name);
    }
}
