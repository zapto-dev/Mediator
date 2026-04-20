using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Zapto.Mediator.Generator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseTypedSenderExtensionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ZM0001";
    public const string ExtensionMethodNameProperty = "ExtensionMethodName";
    public const string HasNamespaceArgProperty = "HasNamespaceArg";
    public const string IsObjectCreationProperty = "IsObjectCreation";

    internal static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Use typed sender extension method",
        messageFormat: "Use '{0}' instead",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Use the typed extension method generated for this request type instead of the generic Send/Publish/CreateStream overload.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodIdentifier = memberAccess.Name.Identifier.ValueText;
        if (methodIdentifier is not ("Send" or "Publish" or "CreateStream"))
            return;

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            return;

        var containingType = methodSymbol.ContainingType;
        if (!IsMediatorInterface(containingType))
            return;

        var args = invocation.ArgumentList.Arguments;
        if (args.Count == 0) return;

        // Named outer arguments can cause incorrect argument reordering in the code fix
        foreach (var arg in args)
        {
            if (arg.NameColon != null) return;
        }

        // The first argument might be a MediatorNamespace for namespaced calls
        var requestArgIndex = 0;
        var firstArgType = context.SemanticModel.GetTypeInfo(args[0].Expression).Type;
        if (firstArgType?.Name == "MediatorNamespace" &&
            firstArgType.ContainingNamespace?.ToDisplayString() == "Zapto.Mediator")
        {
            requestArgIndex = 1;
        }

        if (args.Count <= requestArgIndex) return;

        var requestArg = args[requestArgIndex];
        if (context.SemanticModel.GetTypeInfo(requestArg.Expression).Type is not INamedTypeSymbol requestType)
            return;

        var interfaceName = FindRequestInterface(requestType);
        if (interfaceName is null) return;

        var extensionMethodName = ComputeExtensionMethodName(interfaceName, requestType.Name, methodSymbol);

        if (!ExtensionMethodExists(context.Compilation, requestType, extensionMethodName))
            return;

        // Don't unwrap generic types (type arguments may not be inferable from ctor args alone)
        // Don't unwrap when an object initializer is also present (it would be silently dropped)
        var isObjectCreation = !requestType.IsGenericType &&
            requestArg.Expression is ObjectCreationExpressionSyntax oc &&
            oc.ArgumentList?.Arguments.Count > 0 &&
            oc.Initializer == null;
        var hasNamespaceArg = requestArgIndex > 0;

        var properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties[ExtensionMethodNameProperty] = extensionMethodName;
        properties[HasNamespaceArgProperty] = hasNamespaceArg ? "true" : "false";
        properties[IsObjectCreationProperty] = isObjectCreation ? "true" : "false";

        var diagnostic = Diagnostic.Create(
            Rule,
            memberAccess.Name.GetLocation(),
            properties.ToImmutable(),
            extensionMethodName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsMediatorInterface(INamedTypeSymbol type)
    {
        return type.ContainingNamespace?.ToDisplayString() == "Zapto.Mediator" &&
               type.Name is "ISender" or "IPublisher" or "IBackgroundPublisher";
    }

    internal static string? FindRequestInterface(INamedTypeSymbol requestType)
    {
        foreach (var iface in requestType.AllInterfaces)
        {
            var ns = iface.ContainingNamespace?.ToDisplayString();
            if ((ns == "Zapto.Mediator" || ns == "MediatR") &&
                iface.Name is "IRequest" or "INotification" or "IStreamRequest")
            {
                return iface.Name;
            }
        }
        return null;
    }

    internal static string ComputeExtensionMethodName(string interfaceName, string typeName, IMethodSymbol? methodSymbol = null)
    {
        // Remove the "I" prefix to get suffix: IRequest -> Request, INotification -> Notification
        var suffix = interfaceName.Substring(1);
        var baseName = typeName.EndsWith(suffix) && typeName.Length != suffix.Length
            ? typeName.Substring(0, typeName.Length - suffix.Length)
            : typeName;

        // IBackgroundPublisher methods are void, so no "Async" suffix
        var isVoidReturn = methodSymbol?.ReturnType.SpecialType == SpecialType.System_Void;
        return isVoidReturn ? baseName : baseName + "Async";
    }

    internal static bool ExtensionMethodExists(Compilation compilation, INamedTypeSymbol requestType, string methodName)
    {
        var isGlobalNs = requestType.ContainingNamespace?.IsGlobalNamespace ?? true;
        var ns = isGlobalNs ? null : requestType.ContainingNamespace?.ToDisplayString();
        var fullTypeName = ns is null ? "SenderExtensions" : $"{ns}.SenderExtensions";

        var extensionsType = compilation.GetTypeByMetadataName(fullTypeName);
        if (extensionsType is null) return false;

        foreach (var member in extensionsType.GetMembers(methodName))
        {
            if (member is IMethodSymbol) return true;
        }

        return false;
    }
}
