using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Zapto.Mediator.Generator;

public record Constructor(Accessibility Visibility, ParameterListSyntax Parameters);
