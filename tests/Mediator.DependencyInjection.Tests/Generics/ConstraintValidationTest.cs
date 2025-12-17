using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zapto.Mediator;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace Mediator.DependencyInjection.Tests.Generics;

/// <summary>
/// Tests to validate that generic constraint checking works correctly for all handler types.
/// This ensures handlers with unsatisfied constraints are properly skipped.
/// </summary>
public class ConstraintValidationTest
{
    #region Test Types and Interfaces

    public interface ISpecialInterface { }
    public class ClassWithInterface : ISpecialInterface { }
    public class ClassWithoutInterface { }

    public class BaseClass { }
    public class DerivedClass : BaseClass { }

    #endregion

    #region Notification Tests

    public record TestNotification<T>(T Value) : INotification;

    public class NotificationResult
    {
        public List<string> HandlersCalled { get; } = new();
    }

    // Handler with interface constraint
    public class NotificationHandlerWithInterfaceConstraint<T> : INotificationHandler<TestNotification<T>>
        where T : ISpecialInterface
    {
        private readonly NotificationResult _result;

        public NotificationHandlerWithInterfaceConstraint(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, TestNotification<T> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add("InterfaceConstraint");
            return default;
        }
    }

    // Handler with base class constraint
    public class NotificationHandlerWithBaseClassConstraint<T> : INotificationHandler<TestNotification<T>>
        where T : BaseClass
    {
        private readonly NotificationResult _result;

        public NotificationHandlerWithBaseClassConstraint(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, TestNotification<T> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add("BaseClassConstraint");
            return default;
        }
    }

    // Handler with struct constraint
    public class NotificationHandlerWithStructConstraint<T> : INotificationHandler<TestNotification<T>>
        where T : struct
    {
        private readonly NotificationResult _result;

        public NotificationHandlerWithStructConstraint(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, TestNotification<T> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add("StructConstraint");
            return default;
        }
    }

    // Handler with class constraint
    public class NotificationHandlerWithClassConstraint<T> : INotificationHandler<TestNotification<T>>
        where T : class
    {
        private readonly NotificationResult _result;

        public NotificationHandlerWithClassConstraint(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, TestNotification<T> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add("ClassConstraint");
            return default;
        }
    }

#if NET7_0_OR_GREATER
    // Handler with self-referential constraint (like INumber<T>)
    public class NotificationHandlerWithNumberConstraint<T> : INotificationHandler<TestNotification<T>>
        where T : INumber<T>
    {
        private readonly NotificationResult _result;

        public NotificationHandlerWithNumberConstraint(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, TestNotification<T> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add("NumberConstraint");
            return default;
        }
    }
#endif

    [Fact]
    public async Task Notification_InterfaceConstraint_OnlyMatchesTypesWithInterface()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(NotificationHandlerWithInterfaceConstraint<>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke handler - ClassWithInterface implements ISpecialInterface
        await mediator.Publish(new TestNotification<ClassWithInterface>(new ClassWithInterface()));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("InterfaceConstraint", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke handler - ClassWithoutInterface doesn't implement ISpecialInterface
        await mediator.Publish(new TestNotification<ClassWithoutInterface>(new ClassWithoutInterface()));
        Assert.Empty(result.HandlersCalled);
    }

    [Fact]
    public async Task Notification_BaseClassConstraint_OnlyMatchesDerivedTypes()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(NotificationHandlerWithBaseClassConstraint<>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke handler - DerivedClass inherits from BaseClass
        await mediator.Publish(new TestNotification<DerivedClass>(new DerivedClass()));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("BaseClassConstraint", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should invoke handler - BaseClass itself
        await mediator.Publish(new TestNotification<BaseClass>(new BaseClass()));
        Assert.Single(result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke handler - string doesn't inherit from BaseClass
        await mediator.Publish(new TestNotification<string>("test"));
        Assert.Empty(result.HandlersCalled);
    }

    [Fact]
    public async Task Notification_StructConstraint_OnlyMatchesValueTypes()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(NotificationHandlerWithStructConstraint<>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke handler - int is a struct
        await mediator.Publish(new TestNotification<int>(42));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("StructConstraint", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke handler - string is a reference type
        await mediator.Publish(new TestNotification<string>("test"));
        Assert.Empty(result.HandlersCalled);
    }

    [Fact]
    public async Task Notification_ClassConstraint_OnlyMatchesReferenceTypes()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(NotificationHandlerWithClassConstraint<>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke handler - string is a reference type
        await mediator.Publish(new TestNotification<string>("test"));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("ClassConstraint", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke handler - int is a value type
        await mediator.Publish(new TestNotification<int>(42));
        Assert.Empty(result.HandlersCalled);
    }

#if NET7_0_OR_GREATER
    [Fact]
    public async Task Notification_NumberConstraint_OnlyMatchesNumericTypes()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(NotificationHandlerWithNumberConstraint<>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke handler - int implements INumber<int>
        await mediator.Publish(new TestNotification<int>(42));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("NumberConstraint", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should invoke handler - double implements INumber<double>
        await mediator.Publish(new TestNotification<double>(3.14));
        Assert.Single(result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke handler - string doesn't implement INumber<string>
        await mediator.Publish(new TestNotification<string>("test"));
        Assert.Empty(result.HandlersCalled);
    }
#endif

    [Fact]
    public async Task Notification_MultipleHandlersWithDifferentConstraints_OnlyInvokesMatching()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(NotificationHandlerWithInterfaceConstraint<>));
                b.AddNotificationHandler(typeof(NotificationHandlerWithClassConstraint<>));
                b.AddNotificationHandler(typeof(NotificationHandlerWithStructConstraint<>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // ClassWithInterface: should invoke InterfaceConstraint and ClassConstraint (reference type with interface)
        await mediator.Publish(new TestNotification<ClassWithInterface>(new ClassWithInterface()));
        Assert.Equal(2, result.HandlersCalled.Count);
        Assert.Contains("InterfaceConstraint", result.HandlersCalled);
        Assert.Contains("ClassConstraint", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // int: should only invoke StructConstraint (value type)
        await mediator.Publish(new TestNotification<int>(42));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("StructConstraint", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // ClassWithoutInterface: should only invoke ClassConstraint (reference type without interface)
        await mediator.Publish(new TestNotification<ClassWithoutInterface>(new ClassWithoutInterface()));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("ClassConstraint", result.HandlersCalled);
    }

    #endregion

    #region Request Tests

    public record TestRequest<T>(T Value) : IRequest<T>;

    // Handler with interface constraint for requests
    public class RequestHandlerWithInterfaceConstraint<T> : IRequestHandler<TestRequest<T>, T>
        where T : ISpecialInterface
    {
        public ValueTask<T> Handle(IServiceProvider provider, TestRequest<T> request, CancellationToken cancellationToken)
        {
            return new ValueTask<T>(request.Value);
        }
    }

#if NET7_0_OR_GREATER
    // Handler with numeric constraint for requests
    public class RequestHandlerWithNumberConstraint<T> : IRequestHandler<TestRequest<T>, T>
        where T : INumber<T>
    {
        public ValueTask<T> Handle(IServiceProvider provider, TestRequest<T> request, CancellationToken cancellationToken)
        {
            return new ValueTask<T>(request.Value);
        }
    }
#endif

    [Fact]
    public async Task Request_InterfaceConstraint_OnlyMatchesTypesWithInterface()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddRequestHandler(typeof(RequestHandlerWithInterfaceConstraint<>));
            })
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should work - ClassWithInterface implements ISpecialInterface
        var result = await mediator.Send(new TestRequest<ClassWithInterface>(new ClassWithInterface()));
        Assert.NotNull(result);

        // Should throw - ClassWithoutInterface doesn't implement ISpecialInterface
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
            await mediator.Send(new TestRequest<ClassWithoutInterface>(new ClassWithoutInterface())));
    }

#if NET7_0_OR_GREATER
    [Fact]
    public async Task Request_NumberConstraint_OnlyMatchesNumericTypes()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddRequestHandler(typeof(RequestHandlerWithNumberConstraint<>));
            })
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should work - int implements INumber<int>
        var intResult = await mediator.Send(new TestRequest<int>(42));
        Assert.Equal(42, intResult);

        // Should work - double implements INumber<double>
        var doubleResult = await mediator.Send(new TestRequest<double>(3.14));
        Assert.Equal(3.14, doubleResult);

        // Should throw - string doesn't implement INumber<string>
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
            await mediator.Send(new TestRequest<string>("test")));
    }
#endif

    #endregion

    #region Stream Request Tests

    public record TestStreamRequest<T>(T Value) : IStreamRequest<T>;

    // Handler with interface constraint for stream requests
    public class StreamRequestHandlerWithInterfaceConstraint<T> : IStreamRequestHandler<TestStreamRequest<T>, T>
        where T : ISpecialInterface
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<T> Handle(IServiceProvider provider, TestStreamRequest<T> request, [EnumeratorCancellation] CancellationToken cancellationToken)
#pragma warning restore CS1998
        {
            yield return request.Value;
        }
    }

#if NET7_0_OR_GREATER
    // Handler with numeric constraint for stream requests
    public class StreamRequestHandlerWithNumberConstraint<T> : IStreamRequestHandler<TestStreamRequest<T>, T>
        where T : INumber<T>
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<T> Handle(IServiceProvider provider, TestStreamRequest<T> request, [EnumeratorCancellation] CancellationToken cancellationToken)
#pragma warning restore CS1998
        {
            yield return request.Value;
        }
    }
#endif

    [Fact]
    public async Task StreamRequest_InterfaceConstraint_OnlyMatchesTypesWithInterface()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddStreamRequestHandler(typeof(StreamRequestHandlerWithInterfaceConstraint<>));
            })
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should work - ClassWithInterface implements ISpecialInterface
        var result = await mediator.CreateStream(new TestStreamRequest<ClassWithInterface>(new ClassWithInterface()))
            .FirstOrDefaultAsync();
        Assert.NotNull(result);

        // Should throw - ClassWithoutInterface doesn't implement ISpecialInterface
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
        {
            await foreach (var item in mediator.CreateStream(new TestStreamRequest<ClassWithoutInterface>(new ClassWithoutInterface())))
            {
                // Should not reach here
            }
        });
    }

#if NET7_0_OR_GREATER
    [Fact]
    public async Task StreamRequest_NumberConstraint_OnlyMatchesNumericTypes()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddStreamRequestHandler(typeof(StreamRequestHandlerWithNumberConstraint<>));
            })
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should work - int implements INumber<int>
        var intResult = await mediator.CreateStream(new TestStreamRequest<int>(42)).FirstOrDefaultAsync();
        Assert.Equal(42, intResult);

        // Should work - long implements INumber<long>
        var longResult = await mediator.CreateStream(new TestStreamRequest<long>(100L)).FirstOrDefaultAsync();
        Assert.Equal(100L, longResult);

        // Should throw - string doesn't implement INumber<string>
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
        {
            await foreach (var item in mediator.CreateStream(new TestStreamRequest<string>("test")))
            {
                // Should not reach here
            }
        });
    }
#endif

    #endregion

    #region Closed Generic Handler Tests

    // Test that closed generic handlers (non-generic type definition) work correctly
    public class ClosedGenericNotificationHandler : INotificationHandler<TestNotification<ClassWithInterface>>
    {
        private readonly NotificationResult _result;

        public ClosedGenericNotificationHandler(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, TestNotification<ClassWithInterface> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add("ClosedGeneric");
            return default;
        }
    }

    [Fact]
    public async Task ClosedGenericHandler_IsRegisteredAsConcreteHandler()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler<ClosedGenericNotificationHandler>();
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke the closed generic handler
        await mediator.Publish(new TestNotification<ClassWithInterface>(new ClassWithInterface()));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("ClosedGeneric", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke for other types
        await mediator.Publish(new TestNotification<ClassWithoutInterface>(new ClassWithoutInterface()));
        Assert.Empty(result.HandlersCalled);
    }

    #endregion

    #region Nested Generic Tests

    public record NestedNotification<T>(List<T> Values) : INotification;

    // Handler for nested generic with constraint on inner type
    public class NestedGenericNotificationHandler<T> : INotificationHandler<NestedNotification<T>>
        where T : ISpecialInterface
    {
        private readonly NotificationResult _result;

        public NestedGenericNotificationHandler(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, NestedNotification<T> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add($"NestedGeneric<{typeof(T).Name}>");
            return default;
        }
    }

    [Fact]
    public async Task Notification_NestedGeneric_ConstraintAppliedToInnerType()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(NestedGenericNotificationHandler<>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke handler - List<ClassWithInterface> where inner type satisfies constraint
        await mediator.Publish(new NestedNotification<ClassWithInterface>(new List<ClassWithInterface>()));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("NestedGeneric<ClassWithInterface>", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke handler - List<ClassWithoutInterface> where inner type doesn't satisfy constraint
        await mediator.Publish(new NestedNotification<ClassWithoutInterface>(new List<ClassWithoutInterface>()));
        Assert.Empty(result.HandlersCalled);
    }

    #endregion

    #region Multiple Type Parameters Tests

    public record MultiParamNotification<TKey, TValue>(TKey Key, TValue Value) : INotification;

    // Handler with constraints on multiple type parameters
    public class MultiParamConstraintHandler<TKey, TValue> : INotificationHandler<MultiParamNotification<TKey, TValue>>
        where TKey : ISpecialInterface
        where TValue : struct
    {
        private readonly NotificationResult _result;

        public MultiParamConstraintHandler(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, MultiParamNotification<TKey, TValue> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add($"MultiParam<{typeof(TKey).Name},{typeof(TValue).Name}>");
            return default;
        }
    }

    // Handler with partial constraints (only on one parameter)
    public class PartialConstraintHandler<TKey, TValue> : INotificationHandler<MultiParamNotification<TKey, TValue>>
        where TKey : ISpecialInterface
    {
        private readonly NotificationResult _result;

        public PartialConstraintHandler(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, MultiParamNotification<TKey, TValue> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add($"PartialConstraint<{typeof(TKey).Name},{typeof(TValue).Name}>");
            return default;
        }
    }

    [Fact]
    public async Task Notification_MultipleTypeParameters_AllConstraintsMustBeSatisfied()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(MultiParamConstraintHandler<,>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke - both constraints satisfied (ISpecialInterface + struct)
        await mediator.Publish(new MultiParamNotification<ClassWithInterface, int>(new ClassWithInterface(), 42));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("MultiParam<ClassWithInterface,Int32>", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke - first constraint satisfied but second not (string is not struct)
        await mediator.Publish(new MultiParamNotification<ClassWithInterface, string>(new ClassWithInterface(), "test"));
        Assert.Empty(result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke - second constraint satisfied but first not
        await mediator.Publish(new MultiParamNotification<ClassWithoutInterface, int>(new ClassWithoutInterface(), 42));
        Assert.Empty(result.HandlersCalled);
    }

    [Fact]
    public async Task Notification_MultipleTypeParameters_PartialConstraintsWork()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(PartialConstraintHandler<,>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke - first parameter satisfies constraint, second has no constraint
        await mediator.Publish(new MultiParamNotification<ClassWithInterface, string>(new ClassWithInterface(), "test"));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("PartialConstraint<ClassWithInterface,String>", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should invoke - works with value types too
        await mediator.Publish(new MultiParamNotification<ClassWithInterface, int>(new ClassWithInterface(), 42));
        Assert.Single(result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke - first parameter doesn't satisfy constraint
        await mediator.Publish(new MultiParamNotification<ClassWithoutInterface, string>(new ClassWithoutInterface(), "test"));
        Assert.Empty(result.HandlersCalled);
    }

    [Fact]
    public async Task Notification_MultipleTypeParameters_MultipleHandlersWithDifferentConstraints()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(MultiParamConstraintHandler<,>)); // Both constrained
                b.AddNotificationHandler(typeof(PartialConstraintHandler<,>));    // Only first constrained
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Both handlers should match
        await mediator.Publish(new MultiParamNotification<ClassWithInterface, int>(new ClassWithInterface(), 42));
        Assert.Equal(2, result.HandlersCalled.Count);
        Assert.Contains("MultiParam<ClassWithInterface,Int32>", result.HandlersCalled);
        Assert.Contains("PartialConstraint<ClassWithInterface,Int32>", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Only partial constraint handler should match (string is not struct)
        await mediator.Publish(new MultiParamNotification<ClassWithInterface, string>(new ClassWithInterface(), "test"));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("PartialConstraint<ClassWithInterface,String>", result.HandlersCalled);
    }

    #endregion

    #region Generic Parameter Order Tests

    public record OrderedNotification<T1, T2>(T1 First, T2 Second) : INotification;

    // Handler with specific order of constraints
    public class OrderedConstraintHandler<T1, T2> : INotificationHandler<OrderedNotification<T1, T2>>
        where T1 : struct
        where T2 : class
    {
        private readonly NotificationResult _result;

        public OrderedConstraintHandler(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, OrderedNotification<T1, T2> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add($"Ordered<{typeof(T1).Name},{typeof(T2).Name}>");
            return default;
        }
    }

    [Fact]
    public async Task Notification_GenericParameterOrder_ConstraintsApplyToCorrectParameters()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(OrderedConstraintHandler<,>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke - correct order: struct, class
        await mediator.Publish(new OrderedNotification<int, string>(42, "test"));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("Ordered<Int32,String>", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke - wrong order: class, struct
        await mediator.Publish(new OrderedNotification<string, int>("test", 42));
        Assert.Empty(result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke - both reference types
        await mediator.Publish(new OrderedNotification<string, object>("test", new object()));
        Assert.Empty(result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke - both value types
        await mediator.Publish(new OrderedNotification<int, double>(42, 3.14));
        Assert.Empty(result.HandlersCalled);
    }

    #endregion

    #region Complex Nested Scenarios

    public record ComplexNestedNotification<T>(Dictionary<string, List<T>> Data) : INotification;

    public class ComplexNestedHandler<T> : INotificationHandler<ComplexNestedNotification<T>>
        where T : BaseClass
    {
        private readonly NotificationResult _result;

        public ComplexNestedHandler(NotificationResult result)
        {
            _result = result;
        }

        public ValueTask Handle(IServiceProvider provider, ComplexNestedNotification<T> notification, CancellationToken cancellationToken)
        {
            _result.HandlersCalled.Add($"ComplexNested<{typeof(T).Name}>");
            return default;
        }
    }

    [Fact]
    public async Task Notification_ComplexNestedGenerics_ConstraintValidationWorks()
    {
        var result = new NotificationResult();

        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddNotificationHandler(typeof(ComplexNestedHandler<>));
            })
            .AddSingleton(result)
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should invoke - DerivedClass satisfies BaseClass constraint
        await mediator.Publish(new ComplexNestedNotification<DerivedClass>(new Dictionary<string, List<DerivedClass>>()));
        Assert.Single(result.HandlersCalled);
        Assert.Contains("ComplexNested<DerivedClass>", result.HandlersCalled);

        result.HandlersCalled.Clear();

        // Should NOT invoke - string doesn't inherit from BaseClass
        await mediator.Publish(new ComplexNestedNotification<string>(new Dictionary<string, List<string>>()));
        Assert.Empty(result.HandlersCalled);
    }

    #endregion

    #region Request Multi-Parameter Tests

    public record MultiParamRequest<T1, T2>(T1 Key, T2 Value) : IRequest<string>;

    public class MultiParamRequestHandler<T1, T2> : IRequestHandler<MultiParamRequest<T1, T2>, string>
        where T1 : ISpecialInterface
        where T2 : struct
    {
        public ValueTask<string> Handle(IServiceProvider provider, MultiParamRequest<T1, T2> request, CancellationToken cancellationToken)
        {
            return new ValueTask<string>($"{typeof(T1).Name}-{typeof(T2).Name}");
        }
    }

    [Fact]
    public async Task Request_MultipleTypeParameters_AllConstraintsMustBeSatisfied()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddRequestHandler(typeof(MultiParamRequestHandler<,>));
            })
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should work - both constraints satisfied
        var result = await mediator.Send(new MultiParamRequest<ClassWithInterface, int>(new ClassWithInterface(), 42));
        Assert.Equal("ClassWithInterface-Int32", result);

        // Should throw - first constraint not satisfied
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
            await mediator.Send(new MultiParamRequest<ClassWithoutInterface, int>(new ClassWithoutInterface(), 42)));

        // Should throw - second constraint not satisfied
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
            await mediator.Send(new MultiParamRequest<ClassWithInterface, string>(new ClassWithInterface(), "test")));
    }

    #endregion

    #region Stream Request Multi-Parameter Tests

    public record MultiParamStreamRequest<T1, T2>(T1 Key, T2 Value) : IStreamRequest<string>;

    public class MultiParamStreamRequestHandler<T1, T2> : IStreamRequestHandler<MultiParamStreamRequest<T1, T2>, string>
        where T1 : ISpecialInterface
        where T2 : BaseClass
    {
#pragma warning disable CS1998
        public async IAsyncEnumerable<string> Handle(IServiceProvider provider, MultiParamStreamRequest<T1, T2> request, [EnumeratorCancellation] CancellationToken cancellationToken)
#pragma warning restore CS1998
        {
            yield return $"{typeof(T1).Name}-{typeof(T2).Name}";
        }
    }

    [Fact]
    public async Task StreamRequest_MultipleTypeParameters_AllConstraintsMustBeSatisfied()
    {
        await using var provider = new ServiceCollection()
            .AddMediator(b =>
            {
                b.AddStreamRequestHandler(typeof(MultiParamStreamRequestHandler<,>));
            })
            .BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Should work - both constraints satisfied
        var result = await mediator.CreateStream(new MultiParamStreamRequest<ClassWithInterface, DerivedClass>(new ClassWithInterface(), new DerivedClass()))
            .FirstOrDefaultAsync();
        Assert.Equal("ClassWithInterface-DerivedClass", result);

        // Should throw - first constraint not satisfied
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
        {
            await foreach (var item in mediator.CreateStream(new MultiParamStreamRequest<ClassWithoutInterface, DerivedClass>(new ClassWithoutInterface(), new DerivedClass())))
            {
                // Should not reach here
            }
        });

        // Should throw - second constraint not satisfied
        await Assert.ThrowsAsync<HandlerNotFoundException>(async () =>
        {
            await foreach (var item in mediator.CreateStream(new MultiParamStreamRequest<ClassWithInterface, ClassWithoutInterface>(new ClassWithInterface(), new ClassWithoutInterface())))
            {
                // Should not reach here
            }
        });
    }

    #endregion
}

