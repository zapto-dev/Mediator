using MediatR;

namespace Benchmarks.Models;

public record Ping : IRequest<string>;

public record PingDelegate : IRequest<string>;

public record ReturnGeneric<T>(T Value) : IRequest<T>;
