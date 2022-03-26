using MediatR;

namespace Mediator.DependencyInjection.Tests.Generics;

public record GenericNotification<T>(T Value) : INotification;