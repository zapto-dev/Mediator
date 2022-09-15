using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Zapto.Mediator.Generator;

[Generator]
public class SenderGenerator : IIncrementalGenerator
{
    private static readonly string[] Interfaces =
    {
        "Zapto.Mediator.ISender",
        "Zapto.Mediator.IPublisher"
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclorations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (ctx, _) => ctx.Node);

        IncrementalValueProvider<(Compilation, ImmutableArray<SyntaxNode>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclorations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    public static void Execute(Compilation compilation, ImmutableArray<SyntaxNode> classes, SourceProductionContext context)
    {
        var models = new Dictionary<SyntaxTree, SemanticModel>();
        var requests = new List<ExtensionMethodReference>();
        var handlers = new List<HandlerReference>();

        SemanticModel GetModel(SyntaxNode item)
        {
            var tree = item.SyntaxTree;

            if (!models.TryGetValue(tree, out var model))
            {
                model = compilation.GetSemanticModel(tree);
                models.Add(tree, model);
            }

            return model;
        }

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
                    var type = m.TypeParameters[0].ConstraintTypes[0];

                    return new ExtensionMethod(
                        Type: i,
                        Method: m,
                        ParameterType: (
                            Namespace: type.GetNamespace(),
                            Type: type.Name
                        ),
                        type as INamedTypeSymbol
                    );
                }))
            .ToList();

        var methodsByType = types
            .GroupBy(i => i.ParameterType)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var syntaxNode in classes.Distinct())
        {
            Accessibility accessibility;
            BaseListSyntax? baseList;

            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                accessibility = classDeclaration.Modifiers.GetAccessibility();
                baseList = classDeclaration.BaseList;
            }
            else if (syntaxNode is RecordDeclarationSyntax recordDeclaration)
            {
                accessibility = recordDeclaration.Modifiers.GetAccessibility();
                baseList = recordDeclaration.BaseList;
            }
            else
            {
                continue;
            }

            if (baseList is null)
            {
                continue;
            }

            var symbol = (INamedTypeSymbol) GetModel(syntaxNode).GetDeclaredSymbol(syntaxNode)!;

            foreach (var current in symbol.AllInterfaces)
            {
                if (methodsByType.TryGetValue((current.GetNamespace(), current.Name), out var results) &&
                    results.Any(i => i.Constraint == null || i.Constraint.IsGenericType == current.IsGenericType && i.Constraint.TypeArguments.Length == current.TypeArguments.Length))
                {
                    requests.Add(new ExtensionMethodReference(
                        accessibility,
                        symbol,
                        current,
                        results
                    ));
                }

                if (!symbol.IsAbstract &&
                    current.IsGenericType &&
                    current
                        is { Name: "IRequestHandler", TypeArguments.Length: 2 }
                        or { Name: "INotificationHandler", TypeArguments.Length: 1 }
                        or { Name: "IStreamRequestHandler", TypeArguments.Length: 2 } &&
                    !symbol.GetAttributes().Any(i => i.AttributeClass?.Name == "IgnoreHandlerAttribute"))
                {
                    handlers.Add(new HandlerReference(
                        symbol,
                        current
                    ));
                }
            }
        }

        if (requests.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        var intentDepth = 0;

        void AppendIntend()
        {
            for (var i = 0; i < intentDepth; i++)
            {
                sb.Append("    ");
            }
        }

        bool TryAppendVisibility(Accessibility visibility)
        {
            switch (visibility)
            {
                case Accessibility.Internal:
                    AppendIntend();
                    sb.Append("internal ");
                    return true;
                case Accessibility.Public:
                    AppendIntend();
                    sb.Append("public ");
                    return true;
                default:
                    return false;
            }
        }

        void AppendGenericConstraints(ExtensionMethodReference request)
        {
            if (request.Type.IsGenericType)
            {
                var parameters = request.Type.TypeParameters
                    .Where(i => i.ConstraintTypes.Length > 0 || i.HasReferenceTypeConstraint ||
                                i.HasValueTypeConstraint ||
                                i.HasUnmanagedTypeConstraint || i.HasConstructorConstraint)
                    .ToArray();

                foreach (var parameter in parameters)
                {
                    AppendIntend();
                    sb.Append("where ");
                    sb.Append(parameter.Name);
                    sb.Append(" : ");

                    int j;
                    for (j = 0; j < parameter.ConstraintTypes.Length; j++)
                    {
                        if (j > 1) sb.Append(", ");
                        sb.AppendType(parameter.ConstraintTypes[j], false);
                    }

                    if (parameter.HasReferenceTypeConstraint)
                    {
                        if (j++ > 1) sb.Append(", ");
                        sb.Append("class");
                    }
                    else if (parameter.HasValueTypeConstraint)
                    {
                        if (j++ > 1) sb.Append(", ");
                        sb.Append("struct");
                    }
                    else if (parameter.HasUnmanagedTypeConstraint)
                    {
                        if (j++ > 1) sb.Append(", ");
                        sb.Append("unmanaged");
                    }

                    if (parameter.HasConstructorConstraint)
                    {
                        if (j > 1) sb.Append(", ");
                        sb.Append("new()");
                    }

                    sb.AppendLine();
                }
            }
        }

        void AppendGenerics(ExtensionMethodReference request)
        {
            if (!request.Type.IsGenericType)
            {
                return;
            }
            
            var length = request.Type.TypeParameters.Length;

            sb.Append('<');
            for (var i = 0; i < length; i++)
            {
                var parameter = request.Type.TypeParameters[i];

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

        // Dependency injection
        if (compilation.GetTypeByMetadataName("Zapto.Mediator.ServiceProviderMediator") != null)
        {
            AppendIntend();
            sb.AppendLine("namespace Zapto.Mediator");
            AppendIntend();
            sb.AppendLine("{");
            intentDepth++;
            {
                AppendIntend();
                var assemblyName = compilation.Assembly.Name.Replace('.', '_');
                sb.AppendLine("internal static class AssemblyExtensions_" + assemblyName);
                AppendIntend();
                sb.AppendLine("{");
                intentDepth++;

                const string builder = "global::Zapto.Mediator.IMediatorBuilder";

                AppendIntend();
                sb.AppendLine($"public static {builder} AddAssemblyHandlers(this {builder} builder)");
                AppendIntend();
                sb.AppendLine("{");
                intentDepth++;

                foreach (var handler in handlers)
                {
                    AppendIntend();
                    
                    if (handler.Interface.Name is "IRequestHandler")
                    {
                        sb.Append("builder.AddRequestHandler(typeof(");
                        sb.AppendType(handler.Type, addNullable: false, addGenericNames: false);
                        sb.AppendLine("));");
                    }
                    else if (handler.Interface.Name == "INotificationHandler")
                    {
                        sb.Append("builder.AddNotificationHandler(typeof(");
                        sb.AppendType(handler.Type, addNullable: false, addGenericNames: false);
                        sb.AppendLine("));");
                    }
                    else if (handler.Interface.Name == "IStreamRequestHandler")
                    {
                        sb.Append("builder.AddStreamRequestHandler(typeof(");
                        sb.AppendType(handler.Type, addNullable: false, addGenericNames: false);
                        sb.AppendLine("));");
                    }
                }

                AppendIntend();
                sb.AppendLine("return builder;");
                
                intentDepth--;
                AppendIntend();
                sb.AppendLine("}");
            }
            intentDepth--;
            AppendIntend();
            sb.AppendLine("}");
            
            intentDepth--;
            AppendIntend();
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Send message
        foreach (var group in requests.GroupBy(i => (
            Namespace: i.Type.ContainingNamespace is { Name: null or "" } ? "" : i.Type.ContainingNamespace.ToDisplayString(),
            Assembly: i.Type.ContainingAssembly?.Name ?? ""
        )))
        {
            var hasNamespace = !string.IsNullOrWhiteSpace(group.Key.Namespace);

            intentDepth = hasNamespace ? 1 : 0;

            if (hasNamespace)
            {
                sb.Append("namespace ");
                sb.AppendLine(group.Key.Namespace);
                sb.AppendLine("{");
            }

            AppendIntend();
            var assemblyName = group.Key.Assembly.Replace('.', '_');
            sb.AppendLine($"public static class {assemblyName}SenderExtensions");

            AppendIntend();
            sb.AppendLine("{");

            intentDepth++;

            foreach (var request in group)
            {
                var suffix = request.Interface.Name.RemovePrefix("I");
                var name = request.Type.Name.RemoveSuffix(suffix);
                var typeNames = request.Interface.TypeParameters
                    .Select((t, i) => (t.Name, Value: request.Interface.TypeArguments[i]))
                    .ToDictionary(t => t.Name, t => t.Value);

                foreach (var (type, method, _, _) in request.Methods)
                {
                    var parameterName = method.TypeParameters[0].Name;
                    var skipConstructorWithoutParameters = request.Type.IsValueType && request.Type.Constructors.Any(i => i.Parameters.Length > 0);
                    

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
                                    sb.AppendType(request.Type, false);
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
                    
                    if (TryAppendVisibility(request.Type.DeclaredAccessibility))
                    {
                        sb.Append("static ");
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
                        AppendGenerics(request);
                        sb.Append("(this ");
                        sb.AppendType(type, false);
                        sb.Append(" sender, ");
                        sb.AppendParameterDefinitions(method.Parameters, t =>
                        {
                            if (t.Type.Name == parameterName)
                            {
                                sb.AppendType(request.Type, addNullable: false);
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

                        intentDepth++;

                        AppendGenericConstraints(request);

                        AppendIntend();
                        sb.Append("=> sender.");
                        sb.Append(method.Name);
                        AppendMethodGenerics();
                        sb.Append('(');
                        sb.AppendParameters(method.Parameters);
                        sb.AppendLine(");");
                        intentDepth--;
                    }

                    foreach (var constructor in request.Type.Constructors)
                    {
                        if (skipConstructorWithoutParameters && constructor.Parameters.Length == 0)
                        {
                            continue;
                        }

                        var visibility = constructor.DeclaredAccessibility.GetLowest(request.Accessibility);

                        if (!TryAppendVisibility(visibility))
                        {
                            continue;
                        }

                        sb.Append("static ");
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
                        AppendGenerics(request);
                        sb.Append("(this ");
                        sb.AppendType(type, false);
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

                        intentDepth++;

                        AppendGenericConstraints(request);

                        AppendIntend();
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
                            sb.AppendType(request.Type, false);
                            sb.Append('(');
                            sb.AppendParameters(constructor.Parameters);
                            sb.Append(')');
                            return true;
                        });
                        sb.AppendLine(");");
                        intentDepth--;
                    }
                }
            }

            intentDepth--;

            AppendIntend();
            sb.AppendLine("}");

            if (hasNamespace)
            {
                sb.AppendLine("}");
            }
        }

        context.AddSource("MediatorExtensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
