using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Zapto.Mediator;
using Zapto.Mediator.Generator;

namespace Mediator.SourceGenerator.Tests;

public class AnalyzerTests
{
    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var (compilation, _) = await BuildCompilationAsync(source);
        var analyzer = new UseTypedSenderExtensionAnalyzer();
        var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        var all = await withAnalyzers.GetAnalyzerDiagnosticsAsync();
        return all.Where(d => d.Id == UseTypedSenderExtensionAnalyzer.DiagnosticId).ToImmutableArray();
    }

    private static IEnumerable<MetadataReference> GetPlatformReferences()
    {
        // Use TRUSTED_PLATFORM_ASSEMBLIES to get all .NET platform assemblies.
        // This ensures types like ValueTask and CancellationToken resolve correctly
        // even when referenced libraries target an older TFM (e.g. net8.0).
        var trustedPaths = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            ?.Split(System.IO.Path.PathSeparator) ?? Array.Empty<string>();

        foreach (var path in trustedPaths)
        {
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                yield return MetadataReference.CreateFromFile(path);
        }
    }

    private static async Task<(Compilation Compilation, ImmutableArray<SyntaxTree> GeneratedTrees)> BuildCompilationAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = GetPlatformReferences()
            .Append(MetadataReference.CreateFromFile(typeof(IRequest).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Run the source generator first so that SenderExtensions is available
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SenderGenerator(generateAssemblyInfo: false));
        driver = driver.RunGenerators(compilation);
        var generatedTrees = driver.GetRunResult().GeneratedTrees;
        var updatedCompilation = compilation.AddSyntaxTrees(generatedTrees);

        return await Task.FromResult<(Compilation, ImmutableArray<SyntaxTree>)>((updatedCompilation, generatedTrees));
    }

    private static async Task<string> ApplyCodeFixAsync(string source)
    {
        var (compilation, generatedTrees) = await BuildCompilationAsync(source);

        var analyzer = new UseTypedSenderExtensionAnalyzer();
        var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync();
        var diagnostic = diagnostics.FirstOrDefault(d => d.Id == UseTypedSenderExtensionAnalyzer.DiagnosticId);
        if (diagnostic is null) return source;

        // Apply the code fix using an AdhocWorkspace
        var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var solution = workspace.CurrentSolution
            .AddProject(projectId, "Tests", "Tests", LanguageNames.CSharp)
            .AddMetadataReferences(projectId, compilation.References)
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddDocument(documentId, "Test.cs", compilation.SyntaxTrees.First().GetText());

        // Add generated trees as additional documents
        foreach (var tree in generatedTrees)
        {
            var genDocId = DocumentId.CreateNewId(projectId);
            solution = solution.AddDocument(genDocId, tree.FilePath ?? "generated.cs", tree.GetText());
        }

        var document = solution.GetDocument(documentId)!;
        var fixProvider = new UseTypedSenderExtensionCodeFixProvider();
        Microsoft.CodeAnalysis.CodeActions.CodeAction? action = null;

        var fixContext = new CodeFixContext(
            document,
            diagnostic,
            (a, _) => action = a,
            System.Threading.CancellationToken.None);

        await fixProvider.RegisterCodeFixesAsync(fixContext);
        if (action is null) return source;

        var operations = await action.GetOperationsAsync(System.Threading.CancellationToken.None);
        var changedSolution = operations.OfType<Microsoft.CodeAnalysis.CodeActions.ApplyChangesOperation>().First().ChangedSolution;
        var changedDoc = changedSolution.GetDocument(documentId)!;
        var text = await changedDoc.GetTextAsync();
        return text.ToString();
    }

    [Fact]
    public async Task ReportsDiagnosticForSendWithNewRequest()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public record Request(string Arg) : IRequest;

public class Usage
{
    public async Task Run(ISender sender)
    {
        await sender.Send(new Request(""hello""));
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.Single(diagnostics);
        Assert.Equal(UseTypedSenderExtensionAnalyzer.DiagnosticId, diagnostics[0].Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostics[0].Severity);
        diagnostics[0].Properties.TryGetValue(UseTypedSenderExtensionAnalyzer.ExtensionMethodNameProperty, out var methodName);
        Assert.Equal("RequestAsync", methodName);
    }

    [Fact]
    public async Task ReportsDiagnosticForPublishWithNewNotification()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public record Notification(string Arg) : INotification;

public class Usage
{
    public async Task Run(IPublisher publisher)
    {
        await publisher.Publish(new Notification(""hello""));
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.Single(diagnostics);
        diagnostics[0].Properties.TryGetValue(UseTypedSenderExtensionAnalyzer.ExtensionMethodNameProperty, out var methodName);
        Assert.Equal("NotificationAsync", methodName);
    }

    [Fact]
    public async Task ReportsDiagnosticForSendWithVariable()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public record Request(string Arg) : IRequest;

public class Usage
{
    public async Task Run(ISender sender)
    {
        var req = new Request(""hello"");
        await sender.Send(req);
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Single(diagnostics);
    }

    [Fact]
    public async Task NoDiagnosticWhenExtensionMethodDoesNotExist()
    {
        // IRequest but no source generator has run, so no SenderExtensions
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public class CustomRequest : IRequest
{
}

public class Usage
{
    public async Task Run(ISender sender)
    {
        await sender.Send(new CustomRequest());
    }
}";
        // Build compilation WITHOUT the generator
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { syntaxTree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IRequest).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IMediator).Assembly.Location),
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new UseTypedSenderExtensionAnalyzer();
        var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
        var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync();

        Assert.Empty(diagnostics.Where(d => d.Id == UseTypedSenderExtensionAnalyzer.DiagnosticId));
    }

    [Fact]
    public async Task CodeFixUnwrapsConstructorArguments()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;
using System.Threading;

public record Request(string Arg) : IRequest;

public class Usage
{
    public async Task Run(ISender sender, CancellationToken ct)
    {
        await sender.Send(new Request(""hello""), ct);
    }
}";

        var result = await ApplyCodeFixAsync(source);
        Assert.Contains("RequestAsync(\"hello\", ct)", result);
        Assert.DoesNotContain("sender.Send(", result);
    }

    [Fact]
    public async Task CodeFixPreservesVariableArgument()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public record Request(string Arg) : IRequest;

public class Usage
{
    public async Task Run(ISender sender)
    {
        var req = new Request(""hello"");
        await sender.Send(req);
    }
}";

        var result = await ApplyCodeFixAsync(source);
        Assert.Contains("RequestAsync(req)", result);
        Assert.DoesNotContain("sender.Send(", result);
    }

    [Fact]
    public async Task CodeFixHandlesNamespacedCall()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public record Request(string Arg) : IRequest;

public class Usage
{
    private static readonly MediatorNamespace Ns = new MediatorNamespace(""test"");

    public async Task Run(ISender sender)
    {
        await sender.Send(Ns, new Request(""hello""));
    }
}";

        var result = await ApplyCodeFixAsync(source);
        Assert.Contains("RequestAsync(Ns, \"hello\")", result);
        Assert.DoesNotContain("sender.Send(", result);
    }

    [Fact]
    public async Task NoDiagnosticForOuterNamedArguments()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public record Request(string Arg) : IRequest;

public class Usage
{
    public async Task Run(ISender sender)
    {
        await sender.Send(request: new Request(""hello""));
    }
}";

        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task CodeFixPreservesObjectWithInitializer()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public class Request : IRequest
{
    public string Arg { get; }
    public string Extra { get; init; } = """";
    public Request(string arg) => Arg = arg;
}

public class Usage
{
    public async Task Run(ISender sender)
    {
        await sender.Send(new Request(""hello"") { Extra = ""world"" });
    }
}";

        var result = await ApplyCodeFixAsync(source);
        Assert.Contains("RequestAsync(new Request(\"hello\") { Extra = \"world\" })", result);
        Assert.DoesNotContain("sender.Send(", result);
    }

    [Fact]
    public async Task CodeFixHandlesGenericRequestType()
    {
        const string source = @"
using MediatR;
using Zapto.Mediator;
using System.Threading.Tasks;

public record GenericRequest<T>(T Value) : IRequest;

public class Usage
{
    public async Task Run(ISender sender)
    {
        await sender.Send(new GenericRequest<string>(""hello""));
    }
}";

        var result = await ApplyCodeFixAsync(source);
        // Generic types pass the whole request object to preserve explicit type arguments
        Assert.Contains("GenericAsync(new GenericRequest<string>(\"hello\"))", result);
        Assert.DoesNotContain("sender.Send(", result);
    }
}
