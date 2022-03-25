using MediatR;

namespace Benchmarks.Models;

public record struct Ping : IRequest<string>;

public record struct PingDelegate : IRequest<string>;

public record struct ReturnGeneric<T>(T Value) : IRequest<T>;
