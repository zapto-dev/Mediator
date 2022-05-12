using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zapto.Mediator;
using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;

namespace Mediator.SourceGenerator.Tests;

public static class TestHelper
{
    public static Task Verify<TGenerator>(string source, params Type[] extraTypes)
        where TGenerator : class, IIncrementalGenerator, new()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>()
        {
            MetadataReference.CreateFromFile(typeof(IRequest).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location),
        };

        foreach (var type in extraTypes)
        {
            references.Add(MetadataReference.CreateFromFile(type.Assembly.Location));
        }

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references);


        var generator = new TGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver);
    }
}
