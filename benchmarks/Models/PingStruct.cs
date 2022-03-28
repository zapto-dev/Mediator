using MediatR;

namespace Benchmarks.Models;

public record struct PingStruct : IRequest<string>;

public record struct PingStructDelegate : IRequest<string>;

public record struct ReturnStructGeneric<T>(T Value) : IRequest<T>;
