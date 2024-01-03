using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Zapto.Mediator.Generator;

internal record ExtensionMethodReference(
    Accessibility Accessibility,
    SimpleType Interface,
    EquatableArray<ExtensionMethod> Methods
);