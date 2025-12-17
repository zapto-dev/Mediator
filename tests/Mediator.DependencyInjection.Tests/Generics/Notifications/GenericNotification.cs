using MediatR;

namespace Mediator.DependencyInjection.Tests.Generics;

public record GenericNotification<T>(T Value) : INotification;

public interface IInterface;
public interface IInterfaceA : IInterface;
public interface IInterfaceB : IInterface;

public class ClassImplementingInterface : IInterface;
public class ClassImplementingA : IInterfaceA;
public class ClassImplementingB : IInterfaceB;

public record Wrapper<T>(T Value)
{
    public static implicit operator Wrapper<T>(T value) => new(value);
}