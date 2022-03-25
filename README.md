# Zapto.Mediator
Simple mediator implementation for Microsoft.Extensions.DependencyInjection.

This project is inspired by [MediatR](https://github.com/jbogard/MediatR). It uses [MediatR.Contracts](https://www.nuget.org/packages/MediatR.Contracts) so you can reuse all your contracts.

## Differences
Zapto.Mediator:

1. Only supports `Microsoft.Extensions.DependencyInjection`.
2. Requires you to specify types with `ISender.Send<TRequest, TResponse>(new TRequest())` to avoid boxing.  
   To make it easier you can use Zapto.Mediator.SourceGenerator to generate extension methods (e.g. `ISender.RequestAsync()`).
3. Does **not** support pipelines or generics (this is on the roadmap).
4. Allows you to use [C# 10 delegates](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/lambda-improvements).
5. Allows you to use request namespaces (multiple request handlers under a different namespace).
6. Uses `ValueTask` instead of `Task`.

## Benchmark
Note: [like MediatR](https://github.com/jbogard/MediatR.Extensions.Microsoft.DependencyInjection/blob/master/README.md), all handlers are registered as transient with the exception of delegate handlers.

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000 (Windows 11)
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.201
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
```

### Single scope
This benchmark can be found in [benchmarks/Benchmarks/RequestBenchmark.cs](benchmarks/Benchmarks/RequestBenchmark.cs).

|         Method |      Mean |    Error |   StdDev |  Gen 0 | Allocated |
|--------------- |----------:|---------:|---------:|-------:|----------:|
|        MediatR | 502.52 ns | 2.057 ns | 1.824 ns | 0.0849 |   1,424 B |
|          Zapto |  52.67 ns | 0.195 ns | 0.173 ns | 0.0014 |      24 B |
|  ZaptoDelegate |  59.28 ns | 0.282 ns | 0.250 ns |      - |         - |
| ZaptoNamespace | 101.53 ns | 0.174 ns | 0.154 ns | 0.0086 |     144 B |

### Scoped
This benchmark can be found in [benchmarks/Benchmarks/ScopedRequestBenchmark.cs](benchmarks/Benchmarks/ScopedRequestBenchmark.cs).

|         Method |     Mean |   Error |  StdDev |  Gen 0 | Allocated |
|--------------- |---------:|--------:|--------:|-------:|----------:|
|        MediatR | 651.3 ns | 1.73 ns | 1.35 ns | 0.0992 |   1,664 B |
|          Zapto | 141.2 ns | 2.74 ns | 2.56 ns | 0.0105 |     176 B |
|  ZaptoDelegate | 137.0 ns | 0.56 ns | 0.49 ns | 0.0091 |     152 B |
| ZaptoNamespace | 205.2 ns | 0.80 ns | 0.71 ns | 0.0176 |     296 B |

## Example
```csharp
using MediatR;
using Zapto.Mediator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Add mediator
services.AddMediator();

// Add a request handler for "GetMessage"
services.AddRequestHandler((GetMessage _) => "Hello world");

// Add a notification handler for "WriteLineNotification"
services.AddNotificationHandler((WriteLineNotification notification) =>
{
    Console.WriteLine(notification.Message);
});

// Add a notification handler for "WriteLineNotification" with-in the mediator namespace "upper"
var upperNs = new MediatorNamespace("upper");

services.AddNotificationHandler((WriteLineNotification notification) =>
{
    Console.WriteLine(notification.Message.ToUpper());
}, upperNs);

// Create the service provider and execute the request and notifications
// Note that the extension methods 'GetMessageAsync' and 'WriteLineAsync' are generated by the source generator
await using var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();
var message = await mediator.GetMessageAsync();

await mediator.WriteLineAsync(message);
await mediator.WriteLineAsync(upperNs, message);

public record struct WriteLineNotification(string Message) : INotification;

public record struct GetMessage : IRequest<string>;
```

Result:
```
Hello world
HELLO WORLD
```