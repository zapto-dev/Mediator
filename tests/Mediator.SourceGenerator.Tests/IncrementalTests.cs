using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Zapto.Mediator;
using Zapto.Mediator.Generator;

namespace Mediator.SourceGenerator.Tests;

public class IncrementalGeneratorTests
{
    [Theory]

    [InlineData(
        IncrementalStepRunReason.Unchanged,
        IncrementalStepRunReason.Cached,
        """
        using MediatR;
        
        public record Request<T>(T Argument) : IRequest<string>;
        """,
        """
        using MediatR;
        
        public record Request<T>(T Argument) : IRequest<string>
        {
            public int OptionalArgument { get; set; }
        }
        """)]

    [InlineData(
        IncrementalStepRunReason.Modified,
        IncrementalStepRunReason.Modified,
        """
        using MediatR;

        public record Request<T>(T Argument) : IRequest<string>;
        """,
        """
        using MediatR;

        public record Request<T>(T Argument, int ExtraArgument) : IRequest<string>;
        """)]

    [InlineData(
        IncrementalStepRunReason.Unchanged,
        IncrementalStepRunReason.Modified,
        """
        using MediatR;

        public record Request : IRequest;
        """,
        """
        using MediatR;
        using Zapto.Mediator;
        
        public record Request : IRequest;
        
        public class RequestHandler : IRequestHandler<Request>
        {
            public ValueTask<Unit> Handle(IServiceProvider provider, Request request, CancellationToken cancellationToken)
            {
                return default;
            }
        }
        """)]

    [InlineData(
        IncrementalStepRunReason.Unchanged,
        IncrementalStepRunReason.Unchanged,
        """
        using MediatR;

        public record Request : IRequest;
        """,
        """
        using MediatR;
        using Zapto.Mediator;
        
        public record Request : IRequest;

        public record OtherRequest : IRequest;

        public class OtherRequestHandler : IRequestHandler<OtherRequest>
        {
            public ValueTask<Unit> Handle(IServiceProvider provider, OtherRequest request, CancellationToken cancellationToken)
            {
                return default;
            }
        }
        """)]

    public void CheckGeneratorIsIncremental(
        IncrementalStepRunReason executeStepReason,
        IncrementalStepRunReason combineStepReason,
        string source,
        string sourceUpdated)
    {
        SyntaxTree baseSyntaxTree = CSharpSyntaxTree.ParseText(source);

        Compilation compilation = CSharpCompilation.Create(
            "compilation",
            new[] { baseSyntaxTree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(IRequest).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location),
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        ISourceGenerator sourceGenerator = new SenderGenerator(
            generateAssemblyInfo: false
        ).AsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator },
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true));

        // Run the generator
        driver = driver.RunGenerators(compilation);

        // Update the compilation and rerun the generator
        compilation = compilation.ReplaceSyntaxTree(baseSyntaxTree, CSharpSyntaxTree.ParseText(sourceUpdated));
        driver = driver.RunGenerators(compilation);

        GeneratorRunResult result = driver.GetRunResult().Results.Single();
        Assert.Equal(executeStepReason, result.TrackedSteps["FindClasses"].First().Outputs[0].Reason);
        Assert.Equal(combineStepReason, result.TrackedSteps["FindHandlers"].First().Outputs[0].Reason);
    }
}