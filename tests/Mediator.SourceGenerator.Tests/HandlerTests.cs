using System.Threading.Tasks;
using Zapto.Mediator.Generator;
using VerifyXunit;
using Xunit;

namespace Mediator.SourceGenerator.Tests;

public class HandlerTests
{
    [Fact]
    public Task GenerateHandler()
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

        return TestHelper.Verify<SenderGenerator>(source);
    }
}
