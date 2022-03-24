using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Zapto.Mediator.Generator;

public static class VisibilityExtensions
{
    public static Accessibility GetLowest(this Accessibility self, Accessibility other)
    {
        return (Accessibility) Math.Min((int) self, (int) other);
    }

    public static Accessibility GetAccessibility(this SyntaxTokenList list)
    {
        foreach (var item in list)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (item.Kind())
            {
                case SyntaxKind.PublicKeyword:
                    return Accessibility.Public;
                case SyntaxKind.InternalKeyword:
                    return Accessibility.Internal;
                case SyntaxKind.PrivateKeyword:
                    return Accessibility.Private;
                case SyntaxKind.ProtectedKeyword:
                    return Accessibility.Protected;
            }
        }

        return Accessibility.Internal;
    }
}
