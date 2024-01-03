namespace Zapto.Mediator.Generator;

internal record SourceResult(
    SimpleType Type,
    EquatableArray<ExtensionMethodReference> Requests,
    EquatableArray<HandlerReference> Handlers
);

internal record SourceWithHandlerResult(
    SimpleType Type,
    EquatableArray<(ExtensionMethodReference, SimpleType?)> Requests
);