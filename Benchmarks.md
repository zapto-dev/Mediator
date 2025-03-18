# Compared to MediatR
Note: [like MediatR](https://github.com/jbogard/MediatR.Extensions.Microsoft.DependencyInjection/blob/master/README.md), all handlers are registered as transient with the exception of delegate handlers.

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3476)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.200
  [Host]     : .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2
```

## Results
### Empty Class
```csharp
public record Ping : IRequest<string>;
```

| Method              | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Handler_AsInterface |   1.598 ns | 0.0037 ns | 0.0031 ns |  1.00 |    0.00 |      - |         - |          NA |
| Handler_AsClass     |   1.395 ns | 0.0060 ns | 0.0057 ns |  0.87 |    0.00 |      - |         - |          NA |
| MediatR_Interface   |  92.354 ns | 0.6991 ns | 0.5838 ns | 57.79 |    0.37 | 0.0200 |     336 B |          NA |
| MediatR_Object      | 150.164 ns | 0.9268 ns | 0.8669 ns | 93.97 |    0.55 | 0.0243 |     408 B |          NA |
| Zapto_Generic       |  58.938 ns | 0.3907 ns | 0.3463 ns | 36.88 |    0.22 | 0.0043 |      72 B |          NA |
| Zapto_Interface     |  82.303 ns | 0.1812 ns | 0.1414 ns | 51.50 |    0.13 | 0.0057 |      96 B |          NA |
| Zapto_Object        |  98.160 ns | 0.4481 ns | 0.4192 ns | 61.43 |    0.28 | 0.0057 |      96 B |          NA |

### Empty Struct
```csharp
public record struct Ping : IRequest<string>;
```

| Method              | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Handler_AsInterface |   1.620 ns | 0.0090 ns | 0.0084 ns |  1.00 |    0.01 |      - |         - |          NA |
| Handler_AsClass     |   1.399 ns | 0.0032 ns | 0.0026 ns |  0.86 |    0.00 |      - |         - |          NA |
| MediatR_Interface   |  96.538 ns | 0.5650 ns | 0.5285 ns | 59.58 |    0.43 | 0.0200 |     336 B |          NA |
| MediatR_Object      | 119.866 ns | 1.3861 ns | 1.2965 ns | 73.98 |    0.86 | 0.0243 |     408 B |          NA |
| Zapto_Generic       |  57.964 ns | 0.4733 ns | 0.4428 ns | 35.77 |    0.32 | 0.0043 |      72 B |          NA |
| Zapto_Interface     |  80.184 ns | 0.4094 ns | 0.3630 ns | 49.49 |    0.33 | 0.0057 |      96 B |          NA |
| Zapto_Object        |  95.497 ns | 0.4405 ns | 0.4120 ns | 58.94 |    0.38 | 0.0057 |      96 B |          NA |

## Explanation
### Handler
Without any mediator the request is executed directly from the handler.

#### As class
```csharp
var handler = new RequestHandler();

var response = await handler.Handle(new Request("parameter"), CancellationToken.None);
```

#### As interface
```csharp
IRequestHandler<Request, string> handler = new RequestHandler();

var response = await handler.Handle(new Request("parameter"), CancellationToken.None);
```

### Generic request parameter
With Zapto, it's possible to send a request with the request and response types. This can be used to avoid boxing, especially when using structs.
[MediatR has a pull request open](https://github.com/jbogard/MediatR/pull/673) to implement this feature, but it hasn't been merged because it's ugly to specify all arguments. If you use atleast C# 9, it's possible to use the source generator to automaticly generate extension methods.

**Example**
```csharp
var response = await _mediator.Send<Request, Response>(new Request("parameter"));

// With source generator
var response = await _mediator.RequestAsync("parameter");
```

### Interface
With only the response generic parameter, C# can automaticly detect the response type from the request interface. The downside to this is that you have to find the request type, make a generic type for the handler and box the request.

**Example**
```csharp
var response = await _mediator.Send(new Request("parameter"));
```

### Object
It's possible that the request is only an object. In this case the request and response have to be boxed. This is the worst case secenario.

**Example**
```csharp
object request = new Request("parameter");

var response = await _mediator.Send(request);
```

# Handler registration
It's possible to register handlers different ways.

## Results
| Method             | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Class              |  57.88 ns | 0.240 ns | 0.213 ns |  1.00 |    0.01 | 0.0043 |      72 B |        1.00 |
| Class_Namespace    | 128.26 ns | 0.911 ns | 0.852 ns |  2.22 |    0.02 | 0.0134 |     224 B |        3.11 |
| Delegate           |  58.41 ns | 0.245 ns | 0.229 ns |  1.01 |    0.01 | 0.0029 |      48 B |        0.67 |
| Delegate_Namespace | 118.62 ns | 0.815 ns | 0.762 ns |  2.05 |    0.01 | 0.0119 |     200 B |        2.78 |
| Generic            | 112.39 ns | 0.533 ns | 0.473 ns |  1.94 |    0.01 | 0.0091 |     152 B |        2.11 |

## Explanation
### Class
With the class handler, the implementation is provided in a class.

```csharp
public sealed class PingHandlerZapto : IRequestHandler<Ping, string>
{
    public ValueTask<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<string>("Pong");
    }
}

services.AddRequestHandler<Ping, string, PingHandlerZapto>();
```

### Delegate
With the delagete handler, the implementation is provided through a delegate. This is comparable to ASP.NET Core minimal API.

```csharp
services.AddRequestHandler((PingDelegate _) => "pong");
```

### Namespace
In addition to the class (or delegate) handler, it's possible to add a custom namespace to execute different handlers for the same request.

```csharp
var ns = new MediatorNamespace("CustomNamespace")

services.AddRequestHandler<Ping, string, PingHandlerZapto>(ns);
```

### Generic
When your request type has a generic type, you must provide the implementation through a class handler.

```csharp
public record struct ReturnGeneric<T>(T Value) : IRequest<T>;

public class ReturnStructGenericHandlerZapto<T> : IRequestHandler<ReturnStructGeneric<T>, T>
{
    public ValueTask<T> Handle(ReturnStructGeneric<T> request, CancellationToken cancellationToken)
        => new(request.Value);
}

services.AddRequestHandler(typeof(ReturnStructGenericHandlerZapto<>));
```

# Pipeline behavior
It's possible to add a pipeline behavior to the mediator. This can be used to add logging, validation, etc.

## Results
| Method            | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------ |---------:|--------:|--------:|-------:|----------:|
| MediatR_Interface | 187.0 ns | 2.12 ns | 1.99 ns | 0.0377 |     632 B |
| MediatR_Object    | 212.5 ns | 2.23 ns | 2.09 ns | 0.0420 |     704 B |
| Zapto_Generic     | 129.3 ns | 0.81 ns | 0.76 ns | 0.0234 |     392 B |
| Zapto_Interface   | 168.3 ns | 0.95 ns | 0.89 ns | 0.0234 |     392 B |
| Zapto_Object      | 174.3 ns | 1.15 ns | 1.08 ns | 0.0234 |     392 B |
