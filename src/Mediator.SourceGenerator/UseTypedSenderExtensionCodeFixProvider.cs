using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Zapto.Mediator.Generator;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseTypedSenderExtensionCodeFixProvider)), Shared]
public class UseTypedSenderExtensionCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(UseTypedSenderExtensionAnalyzer.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics[0];
        if (!diagnostic.Properties.TryGetValue(UseTypedSenderExtensionAnalyzer.ExtensionMethodNameProperty, out var methodName) || methodName is null)
            return;

        var nameNode = root.FindNode(diagnostic.Location.SourceSpan) as SimpleNameSyntax;
        var memberAccess = nameNode?.Parent as MemberAccessExpressionSyntax;
        var invocation = memberAccess?.Parent as InvocationExpressionSyntax;
        if (invocation is null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Use '{methodName}'",
                createChangedDocument: ct => ApplyFixAsync(context.Document, invocation, methodName, diagnostic.Properties, ct),
                equivalenceKey: UseTypedSenderExtensionAnalyzer.DiagnosticId),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        string methodName,
        ImmutableDictionary<string, string?> properties,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        var hasNamespaceArg = properties.TryGetValue(UseTypedSenderExtensionAnalyzer.HasNamespaceArgProperty, out var nsStr) && nsStr == "true";
        var isObjectCreation = properties.TryGetValue(UseTypedSenderExtensionAnalyzer.IsObjectCreationProperty, out var ocStr) && ocStr == "true";

        var requestArgIndex = hasNamespaceArg ? 1 : 0;
        var args = invocation.ArgumentList.Arguments;
        var newArgs = new System.Collections.Generic.List<ArgumentSyntax>();

        // Preserve the optional namespace argument
        if (hasNamespaceArg)
        {
            newArgs.Add(args[0].WithoutTrivia().WithLeadingTrivia(args[0].GetLeadingTrivia()));
        }

        var requestArg = args[requestArgIndex];

        if (isObjectCreation && requestArg.Expression is ObjectCreationExpressionSyntax oc && oc.ArgumentList is not null)
        {
            // Unwrap constructor arguments: Send(new Req(a, b)) -> ReqAsync(a, b)
            foreach (var ctorArg in oc.ArgumentList.Arguments)
            {
                newArgs.Add(ctorArg);
            }
        }
        else
        {
            // Keep the request expression as-is: Send(req) -> ReqAsync(req)
            newArgs.Add(requestArg);
        }

        // Preserve trailing arguments (e.g. CancellationToken)
        for (var i = requestArgIndex + 1; i < args.Count; i++)
        {
            newArgs.Add(args[i]);
        }

        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
        var newMemberAccess = memberAccess.WithName(
            SyntaxFactory.IdentifierName(methodName)
                .WithTriviaFrom(memberAccess.Name));

        var newInvocation = invocation
            .WithExpression(newMemberAccess)
            .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(newArgs)));

        var newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
