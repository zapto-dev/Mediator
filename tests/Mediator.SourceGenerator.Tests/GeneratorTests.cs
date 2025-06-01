using System.Threading.Tasks;
using VerifyTests;
using Zapto.Mediator.Generator;
using VerifyXunit;
using Xunit;

namespace Mediator.SourceGenerator.Tests;

public class GeneratorTests
{
    [Fact]
    public Task GenerateRequest()
    {
        const string source = @"
using MediatR;

public record Request(string Argument, int OptionalArgument = 0) : IRequest;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateStream()
    {
        const string source = @"
using MediatR;

public record StreamRequest(string Argument, int OptionalArgument = 0) : IStreamRequest<int>;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateNotification()
    {
        const string source = @"
using MediatR;

public record Notification(string Argument, int OptionalArgument = 0) : INotification;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateNotificationNoArguments()
    {
        const string source = @"
using MediatR;

public record Notification : INotification;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateStructNotification()
    {
        const string source = @"
using MediatR;

public record struct Notification(string Argument, int OptionalArgument = 0) : INotification;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateGeneric()
    {
        const string source = @"
using MediatR;

public record Request<T>(T Argument) : IRequest<T>;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateGenericDifferentResult()
    {
        const string source = @"
using MediatR;

public record Request<T>(T Argument) : IRequest<string>;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateGenericConstraint()
    {
        const string source = @"
using MediatR;

public record Request<T>(T Argument) : IRequest<T> where T : IRequest;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateGenericConstraintClass()
    {
        const string source = @"
using MediatR;

public record Request<T>(T Argument) : IRequest<T> where T : class;";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateClassWithoutRequired()
    {
        const string source = @"
using MediatR;

public class Request : IRequest
{
    public string Property { get; set; }
}
";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateClassWithRequired()
    {
        const string source = @"
using MediatR;

public class Request : IRequest
{
    public required string RequiredProperty { get; set; }
}
";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateClassWithMultipleRequired()
    {
        const string source = @"
using MediatR;

public class Request : IRequest
{
    public required string RequiredProperty { get; set; }

    public required int AnotherRequiredProperty { get; set; }
}
";

        return TestHelper.Verify<SenderGenerator>(source);
    }

    [Fact]
    public Task GenerateClassWithOptionalConstructorAndRequired()
    {
        const string source = @"
using MediatR;

public class Request(string optionalParameter = ""a"") : IRequest
{
    public required string RequiredProperty { get; set; }
}
";

        return TestHelper.Verify<SenderGenerator>(source);
    }
}
