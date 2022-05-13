using System.Threading.Tasks;
using Zapto.Mediator.Generator;
using VerifyXunit;
using Xunit;

namespace Mediator.SourceGenerator.Tests;

[UsesVerify]
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
}
