using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Zapto.Mediator.Generator;

public record struct ExtensionMethod(
    INamedTypeSymbol Type,
    IMethodSymbol Method,
    (string Namespace, string Type) ParameterType,
    INamedTypeSymbol? Constraint
);

public record ExtensionMethodReference(
    Accessibility Accessibility,
    INamedTypeSymbol Type,
    INamedTypeSymbol Interface,
    List<ExtensionMethod> Methods
);


public record HandlerReference(
    INamedTypeSymbol Type,
    INamedTypeSymbol Interface
);
