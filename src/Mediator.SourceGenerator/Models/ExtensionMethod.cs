namespace Zapto.Mediator.Generator;

internal record struct ExtensionMethod(
    SimpleType ContainingMethod,
    SimpleType Type,
    SimpleMethod Method,
    (string Namespace, string Type) ParameterType
);