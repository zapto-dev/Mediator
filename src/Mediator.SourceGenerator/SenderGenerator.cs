using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Zapto.Mediator.Generator;

[Generator]
public class SenderGenerator : IIncrementalGenerator
{
    private bool _generateAssemblyInfo;

    private static readonly string[] Interfaces =
    {
        "Zapto.Mediator.ISender",
        "Zapto.Mediator.IPublisherBase"
    };

    public SenderGenerator()
        : this(generateAssemblyInfo: true)
    {
    }

    public SenderGenerator(bool generateAssemblyInfo)
    {
        _generateAssemblyInfo = generateAssemblyInfo;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<SourceResult> results = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (ctx, _) => GetResult(ctx))
            .WithTrackingName("FindClasses")
            .Where(static i => i is not null)!;

        var handlers = results
            .SelectMany(GetAllHandlers)
            .Collect();

        var resultsWithHandlers = results
            .Combine(handlers)
            .Select(static (ctx, _) =>
            {
                var builder = ImmutableArray.CreateBuilder<(ExtensionMethodReference, SimpleType?)>(ctx.Left.Requests.Length);
                var requestType = ctx.Left.Type;

                foreach (var request in ctx.Left.Requests)
                {
                    var (handlerType, _) = ctx.Right.FirstOrDefault(i =>
                        request.Interface.Name == "IRequest" &&
                        i.Handler.Interface.Name == "IRequestHandler" &&
                        i.Handler.Interface.TypeArguments[0].SimpleEquals(requestType) &&
                        (
                            i.Handler.Interface.TypeArguments.Length == 1 ||
                            i.Handler.Interface.TypeArguments[1].SimpleEquals(request.Interface.TypeArguments[0])
                        ));

                    builder.Add((request, handlerType));
                }

                return new SourceWithHandlerResult(
                    Type: ctx.Left.Type,
                    Requests: builder.ToImmutable()
                );
            })
            .WithTrackingName("FindHandlers")!;

        if (_generateAssemblyInfo)
        {
            context.RegisterSourceOutput(handlers, static (spc, source) => CreateAssemblyInfo(spc, source));
        }

        context.RegisterSourceOutput(resultsWithHandlers, static (spc, source) => CreateSource(spc, source));
    }

    private ImmutableArray<(SimpleType Type, HandlerReference Handler)> GetAllHandlers(SourceResult result, CancellationToken _)
    {
        if (result.Handlers.IsEmpty)
        {
            return ImmutableArray<(SimpleType, HandlerReference)>.Empty;
        }

        var builder = new EquatableArrayBuilder<(SimpleType, HandlerReference)>(result.Handlers.Length);

        foreach (var handler in result.Handlers)
        {
            builder.Add((result.Type, handler));
        }

        return builder.ToEquatableArray();
    }

    private static void CreateAssemblyInfo(SourceProductionContext spc, ImmutableArray<(SimpleType, HandlerReference)> handlers)
    {
        if (handlers.IsEmpty)
        {
            return;
        }

        const string builder = "global::Zapto.Mediator.IMediatorBuilder";

        var sb = new IndentedStringBuilder();

        sb.AppendLine();

        using (sb.CodeBlock("namespace Zapto.Mediator"))
        using (sb.CodeBlock("internal static class AssemblyExtensions"))
        using (sb.CodeBlock($"public static {builder} AddAssemblyHandlers(this {builder} builder)"))
        {
            foreach (var (type, handler) in handlers)
            {
                if (handler.Interface.Name is "IRequestHandler")
                {
                    sb.Append("builder.AddRequestHandler(typeof(");
                    sb.AppendType(type, addNullable: false, addGenericNames: false);
                    sb.AppendLine("));");
                }
                else if (handler.Interface.Name == "INotificationHandler")
                {
                    sb.Append("builder.AddNotificationHandler(typeof(");
                    sb.AppendType(type, addNullable: false, addGenericNames: false);
                    sb.AppendLine("));");
                }
                else if (handler.Interface.Name == "IStreamRequestHandler")
                {
                    sb.Append("builder.AddStreamRequestHandler(typeof(");
                    sb.AppendType(type, addNullable: false, addGenericNames: false);
                    sb.AppendLine("));");
                }
            }

            sb.AppendLine("return builder;");
        }

        spc.AddSource("AssemblyExtensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static SourceResult? GetResult(GeneratorSyntaxContext context)
    {
        Accessibility accessibility;
        BaseListSyntax? baseList;

        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            accessibility = classDeclaration.Modifiers.GetAccessibility();
            baseList = classDeclaration.BaseList;
        }
        else if (context.Node is RecordDeclarationSyntax recordDeclaration)
        {
            accessibility = recordDeclaration.Modifiers.GetAccessibility();
            baseList = recordDeclaration.BaseList;
        }
        else
        {
            return null;
        }

        if (baseList is null)
        {
            return null;
        }

        var compilation = context.SemanticModel.Compilation;
        Dictionary<(string Namespace, string Type), EquatableArray<ExtensionMethod>>? methodsByType = null;

        var symbol = (INamedTypeSymbol) context.SemanticModel.GetDeclaredSymbol(context.Node)!;

        var requests = new EquatableArrayBuilder<ExtensionMethodReference>();
        var handlers = new EquatableArrayBuilder<HandlerReference>();

        foreach (var current in symbol.AllInterfaces)
        {
            if (current.Name is ("IRequest" or "INotification" or "IStreamRequest") &&
                (methodsByType ??= GetInterfaceMethods(compilation)).TryGetValue((current.GetNamespace(), GetName(current)), out var results) &&
                results.Any(i => i.Type.IsGenericType == current.IsGenericType && i.Type.TypeArguments.Length == current.TypeArguments.Length))
            {
                requests.Add(new ExtensionMethodReference(
                    accessibility,
                    SimpleType.FromSymbol(current),
                    results
                ));
            }

            if (!symbol.IsAbstract &&
                current.IsGenericType &&
                current
                    is { Name: "IRequestHandler", TypeArguments.Length: 2 }
                    or { Name: "IRequestHandler", TypeArguments.Length: 1 }
                    or { Name: "INotificationHandler", TypeArguments.Length: 1 }
                    or { Name: "IStreamRequestHandler", TypeArguments.Length: 2 } &&
                !symbol.GetAttributes().Any(i => i.AttributeClass?.Name == "IgnoreHandlerAttribute"))
            {
                handlers.Add(new HandlerReference(
                    SimpleType.FromSymbol(current)
                ));
            }
        }

        if (requests.Count == 0 && handlers.Count == 0)
        {
            return null;
        }

        return new SourceResult(
            Type: SimpleType.FromSymbol(symbol, withConstructors: true),
            requests.ToEquatableArray(),
            handlers.ToEquatableArray()
        );
    }

    private static string GetName(INamedTypeSymbol type)
    {
        return type.Name + (type.IsGenericType ? "`" + type.TypeArguments.Length : "");
    }

    private static Dictionary<(string Namespace, string Type), EquatableArray<ExtensionMethod>> GetInterfaceMethods(Compilation compilation)
    {
        var types = Interfaces
            .Select(i => compilation.GetTypeByMetadataName(i)!)
            .Where(i => i is not null)
            .SelectMany(i => i.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(j => j.IsGenericMethod &&
                            j.TypeParameters.Length > 0 &&
                            j.TypeParameters[0].ConstraintTypes.Length > 0)
                .Select(m =>
                {
                    var type = (INamedTypeSymbol) m.TypeParameters[0].ConstraintTypes[0];

                    return new ExtensionMethod(
                        ContainingMethod: SimpleType.FromSymbol(m.ContainingType),
                        Type: SimpleType.FromSymbol(type),
                        Method: SimpleMethod.FromSymbol(m),
                        ParameterType: (
                            Namespace: type.GetNamespace(),
                            Type: GetName(type)
                        )
                    );
                }));

        var methodsByType = types
            .GroupBy(i => i.ParameterType)
            .ToDictionary(g => g.Key, g => new EquatableArray<ExtensionMethod>(g.ToArray()));

        return methodsByType;
    }

    internal static void CreateSource(SourceProductionContext context, SourceWithHandlerResult result)
    {
        if (result.Requests.IsEmpty)
        {
            return;
        }

        var sb = new IndentedStringBuilder();

        bool TryGetVisibility(Accessibility visibility, [NotNullWhen(true)] out string? value)
        {
            switch (visibility)
            {
                case Accessibility.Internal:
                    value = "internal";
                    return true;
                case Accessibility.Public:
                    value = "public";
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        void AppendGenerics()
        {
            if (!result.Type.IsGenericType)
            {
                return;
            }

            var length = result.Type.TypeParameters.Length;

            sb.Append('<');
            for (var i = 0; i < length; i++)
            {
                var parameter = result.Type.TypeParameters[i];

                sb.Append(parameter.Name);

                if (i != length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append('>');
        }

        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        // Send message
        if (result.Type.Namespace is not null)
        {
            sb.Append("namespace ");
            sb.AppendLine(result.Type.Namespace);
            sb.AppendLine("{");
            sb.Indent();
        }

        sb.AppendLine("public static partial class SenderExtensions");
        sb.AppendLine("{");

        sb.Indent();

        foreach (var (request, handlerType) in result.Requests)
        {
            void AddXmlDoc()
            {
                if (handlerType != null)
                {
                    sb.AppendLine("/// <summary>");
                    sb.Append("/// Sends a request to the handler <see cref=\"");
                    sb.AppendType(handlerType, addNullable: false);
                    sb.AppendLine("\"/>.");
                    sb.AppendLine("/// </summary>");
                }
            }

            var suffix = request.Interface.Name.RemovePrefix("I");
            var name = result.Type.Name.RemoveSuffix(suffix);
            var typeNames = request.Interface.TypeParameters
                .Select((t, i) => (t.Name, Value: request.Interface.TypeArguments[i]))
                .ToDictionary(t => t.Name, t => t.Value);

            for (var i = 0; i < request.Methods.Length; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                var (sender, type, method, _) = request.Methods[i];
                var parameterName = method.TypeParameters[0].Name;
                var skipConstructorWithoutParameters = result.Type.IsValueType &&
                                                       result.Type.Constructors.Any(i => i.Parameters.Length > 0);

                void AppendMethodGenerics()
                {
                    if (method.IsGenericMethod)
                    {
                        var length = method.TypeParameters.Length;

                        sb.Append('<');
                        for (var i = 0; i < length; i++)
                        {
                            var parameter = method.TypeParameters[i];

                            if (parameter.Name == parameterName)
                            {
                                sb.AppendType(result.Type, false);
                            }
                            else if (typeNames.TryGetValue(parameter.Name, out var result))
                            {
                                sb.AppendType(result, false);
                            }

                            if (i != length - 1)
                            {
                                sb.Append(", ");
                            }
                        }

                        sb.Append('>');
                    }
                }

                if (TryGetVisibility(result.Type.DeclaredAccessibility, out var visibilityValue))
                {
                    AddXmlDoc();
                    sb.AppendLine("[global::System.Diagnostics.DebuggerStepThrough]");
                    sb.Append(visibilityValue);
                    sb.Append(" static ");
                    sb.AppendType(method.ReturnType,
                        addNullable: false,
                        middleware: t =>
                        {
                            if (!typeNames.TryGetValue(t.Name, out var result))
                            {
                                return false;
                            }

                            sb.AppendType(result, addNullable: false);
                            return true;
                        });
                    sb.Append(' ');
                    sb.Append(name);
                    sb.Append("Async");
                    AppendGenerics();
                    sb.Append("(this ");
                    sb.AppendType(sender, false);
                    sb.Append(" sender, ");
                    sb.AppendParameterDefinitions(method.Parameters, t =>
                    {
                        if (t.Type.Name == parameterName)
                        {
                            sb.AppendType(result.Type, addNullable: false);
                            return AppendResult.Type;
                        }

                        if (typeNames.TryGetValue(t.Type.Name, out var typeName))
                        {
                            sb.AppendType(typeName);
                            return AppendResult.Type;
                        }

                        return AppendResult.None;
                    });
                    sb.AppendLine(")");

                    sb.Indent();
                    sb.AppendGenericConstraints(result.Type);
                    sb.Dedent();

                    sb.Append("=> sender.");
                    sb.Append(method.Name);
                    AppendMethodGenerics();
                    sb.Append('(');
                    sb.AppendParameters(method.Parameters);
                    sb.AppendLine(");");
                }

                foreach (var constructor in result.Type.Constructors)
                {
                    if (skipConstructorWithoutParameters && constructor.Parameters.Length == 0)
                    {
                        continue;
                    }

                    var visibility = constructor.DeclaredAccessibility.GetLowest(request.Accessibility);

                    if (!TryGetVisibility(visibility, out visibilityValue))
                    {
                        continue;
                    }

                    sb.AppendLine();
                    AddXmlDoc();
                    sb.AppendLine("[global::System.Diagnostics.DebuggerStepThrough]");
                    sb.Append(visibilityValue);
                    sb.Append(" static ");
                    sb.AppendType(method.ReturnType,
                        addNullable: false,
                        middleware: t =>
                        {
                            if (!typeNames.TryGetValue(t.Name, out var result))
                            {
                                return false;
                            }

                            sb.AppendType(result, addNullable: false);
                            return true;
                        });

                    sb.Append(' ');
                    sb.Append(name);
                    sb.Append("Async");
                    AppendGenerics();
                    sb.Append("(this ");
                    sb.AppendType(sender, false);
                    sb.Append(" sender, ");
                    sb.AppendParameterDefinitions(method.Parameters, t =>
                    {
                        if (t.Type.Name == parameterName)
                        {
                            sb.AppendParameterDefinitions(constructor.Parameters);
                            return AppendResult.TypeAndName;
                        }

                        if (typeNames.TryGetValue(t.Type.Name, out var typeName))
                        {
                            sb.AppendType(typeName);
                            return AppendResult.Type;
                        }

                        return AppendResult.None;
                    });
                    sb.AppendLine(")");

                    sb.Indent();
                    sb.AppendGenericConstraints(result.Type);
                    sb.Dedent();

                    sb.Append("=> sender.");
                    sb.Append(method.Name);
                    AppendMethodGenerics();

                    sb.Append('(');
                    sb.AppendParameters(method.Parameters, t =>
                    {
                        if (t.Type.Name != parameterName)
                        {
                            return false;
                        }

                        sb.Append("new ");
                        sb.AppendType(result.Type, false);
                        sb.Append('(');
                        sb.AppendParameters(constructor.Parameters);
                        sb.Append(')');
                        return true;
                    });
                    sb.AppendLine(");");
                }
            }
        }

        sb.Dedent();
        sb.AppendLine("}");

        if (result.Type.Namespace is not null)
        {
            sb.Dedent();
            sb.AppendLine("}");
        }

        context.AddSource($"{result.Type.UniqueId}_Extensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
