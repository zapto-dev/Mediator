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
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(IRequest).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location),
            });


        var generator = new TGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver);
    }
}
