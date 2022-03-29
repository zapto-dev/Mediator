# Compared to MediatR
Note: [like MediatR](https://github.com/jbogard/MediatR.Extensions.Microsoft.DependencyInjection/blob/master/README.md), all handlers are registered as transient with the exception of delegate handlers.

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000 (Windows 11)
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=6.0.201
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
```

## Results
### Empty Class
```csharp
public record Ping : IRequest<string>;
```

|              Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
|-------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|
| Handler_AsInterface |  11.059 ns | 0.0446 ns | 0.0372 ns |  1.00 |    0.00 | 0.0014 |      24 B |
|     Handler_AsClass |   7.724 ns | 0.0321 ns | 0.0251 ns |  0.70 |    0.00 |      - |         - |
|   MediatR_Interface | 504.845 ns | 2.0148 ns | 1.7860 ns | 45.66 |    0.18 | 0.0849 |   1,424 B |
|      MediatR_Object | 541.339 ns | 2.7988 ns | 2.4810 ns | 48.93 |    0.26 | 0.0887 |   1,496 B |
|       Zapto_Generic |  35.229 ns | 0.1626 ns | 0.1441 ns |  3.18 |    0.02 | 0.0029 |      48 B |
|     Zapto_Interface |  64.481 ns | 0.4158 ns | 0.3472 ns |  5.83 |    0.03 | 0.0081 |     136 B |
|        Zapto_Object |  85.953 ns | 0.5875 ns | 0.5495 ns |  7.77 |    0.06 | 0.0081 |     136 B |

### Empty Struct
```csharp
public record struct Ping : IRequest<string>;
```

|              Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
|-------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|
| Handler_AsInterface |   9.463 ns | 0.0331 ns | 0.0309 ns |  1.00 |    0.00 |      - |         - |
|     Handler_AsClass |   9.583 ns | 0.0346 ns | 0.0324 ns |  1.01 |    0.00 |      - |         - |
|   MediatR_Interface | 509.057 ns | 2.5120 ns | 2.2268 ns | 53.81 |    0.29 | 0.0849 |   1,424 B |
|      MediatR_Object | 530.815 ns | 4.1636 ns | 3.6909 ns | 56.12 |    0.45 | 0.0887 |   1,496 B |
|       Zapto_Generic |  34.860 ns | 0.1928 ns | 0.1709 ns |  3.69 |    0.03 | 0.0014 |      24 B |
|     Zapto_Interface |  63.125 ns | 0.5586 ns | 0.4952 ns |  6.67 |    0.05 | 0.0081 |     136 B |
|        Zapto_Object |  82.246 ns | 0.5049 ns | 0.4723 ns |  8.69 |    0.05 | 0.0081 |     136 B |

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
|             Method |      Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
|------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|
|              Class |  33.79 ns | 0.168 ns | 0.157 ns |  1.00 |    0.00 | 0.0014 |      24 B |
|    Class_Namespace |  87.82 ns | 1.062 ns | 0.993 ns |  2.60 |    0.02 | 0.0086 |     144 B |
|           Delegate |  34.29 ns | 0.094 ns | 0.088 ns |  1.01 |    0.00 |      - |         - |
| Delegate_Namespace |  64.41 ns | 0.476 ns | 0.445 ns |  1.91 |    0.01 | 0.0072 |     120 B |
|            Generic | 133.90 ns | 0.568 ns | 0.503 ns |  3.96 |    0.02 | 0.0057 |      96 B |

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