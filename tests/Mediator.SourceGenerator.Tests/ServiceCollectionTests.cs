using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zapto.Mediator.Generator;
using VerifyXunit;
using Xunit;

namespace Mediator.SourceGenerator.Tests;

public class ServiceCollectionTests
{
    [Fact]
    public Task GenerateCollection()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;

public record Request : IRequest;

public class RequestHandler : IRequestHandler<Request>
{
    public ValueTask<Unit> Handle(IServiceProvider provider, Request request, CancellationToken cancellationToken)
    {
        return default;
    }
}";

        return TestHelper.Verify<SenderGenerator>(source, typeof(Zapto.Mediator.ServiceProviderMediator));
    }
    
    [Fact]
    public Task GenerateGenericCollection()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;

public record Request<T> : IRequest;

public class RequestHandler<T> : IRequestHandler<Request<T>>
{
    public ValueTask<Unit> Handle(IServiceProvider provider, Request<T> request, CancellationToken cancellationToken)
    {
        return default;
    }
}";

        return TestHelper.Verify<SenderGenerator>(source, typeof(Zapto.Mediator.ServiceProviderMediator));
    }
    
    [Fact]
    public void RegisterHandlerWithMultipleInterfacesOnce()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;

public record NotificationA : INotification;

public record NotificationB : INotification;

public record NotificationC : INotification;

public class NotificationHandler : INotificationHandler<NotificationA>, INotificationHandler<NotificationB>, INotificationHandler<NotificationC>
{
    public ValueTask Handle(IServiceProvider provider, NotificationA notification, CancellationToken cancellationToken) => default;
    public ValueTask Handle(IServiceProvider provider, NotificationB notification, CancellationToken cancellationToken) => default;
    public ValueTask Handle(IServiceProvider provider, NotificationC notification, CancellationToken cancellationToken) => default;
}";

        var result = TestHelper.Run<SenderGenerator>(source, typeof(Zapto.Mediator.ServiceProviderMediator)).GetRunResult();
        var assemblyExtensions = result.GeneratedTrees
            .Single(i => i.FilePath.EndsWith("AssemblyExtensions.g.cs"))
            .ToString();

        Assert.Single(Regex.Matches(assemblyExtensions, Regex.Escape("builder.AddNotificationHandler(typeof(global::NotificationHandler));")));
    }

    [Fact]
    public Task IgnoreHandlerAttribute()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;

public record Request : IRequest;

[IgnoreHandler]
public class RequestHandler : IRequestHandler<Request>
{
    public ValueTask<Unit> Handle(IServiceProvider provider, Request request, CancellationToken cancellationToken)
    {
        return default;
    }
}";

        return TestHelper.Verify<SenderGenerator>(source, typeof(Zapto.Mediator.ServiceProviderMediator));
    }


    [Fact]
    public Task GenerateEnumDefault()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;

public enum MyEnum
{
    First,
    Second
}

public record Request(MyEnum Test = MyEnum.First) : IRequest;
";

        return TestHelper.Verify<SenderGenerator>(source, typeof(Zapto.Mediator.ServiceProviderMediator));
    }
}
