using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;

namespace Mediator.SourceGenerator.Tests;

public static class TestHelper
{
    public static Task Verify<TGenerator>(string source)
        where TGenerator : class, IIncrementalGenerator, new()
    {
        // Parse the provided string into a C# syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create a Roslyn compilation for the syntax tree.
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(IRequest).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location),
            });


        // Create an instance of our EnumGenerator incremental source generator
        var generator = new TGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver);
    }
}
