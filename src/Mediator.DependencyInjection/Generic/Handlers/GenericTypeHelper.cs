using System;
using System.Collections.Generic;

namespace Zapto.Mediator;

internal static class GenericTypeHelper
{
    /// <summary>
    /// Checks if a generic type definition can be instantiated with the given type arguments
    /// by validating all generic constraints.
    /// </summary>
    public static bool CanMakeGenericType(Type genericTypeDefinition, Type[] typeArguments)
    {
        if (!genericTypeDefinition.IsGenericTypeDefinition)
        {
            return false;
        }

        var genericParams = genericTypeDefinition.GetGenericArguments();

        if (genericParams.Length != typeArguments.Length)
        {
            return false;
        }

        // Try to actually make the generic type to let the CLR validate constraints
        // This is the most reliable way to check all constraints including complex ones
        try
        {
            var constructedType = genericTypeDefinition.MakeGenericType(typeArguments);
            return constructedType != null;
        }
        catch (ArgumentException)
        {
            // MakeGenericType throws ArgumentException when constraints are violated
            return false;
        }
        catch (NotSupportedException)
        {
            // MakeGenericType throws NotSupportedException for certain invalid scenarios
            return false;
        }
    }

    /// <summary>
    /// Caches generic registrations for a specific type to avoid repeated enumeration.
    /// </summary>
    public static List<T> CacheMatchingRegistrations<T>(
        IEnumerable<T> registrations,
        Func<T, Type> getNotificationType,
        Type targetGenericType)
    {
        var cached = new List<T>();
        foreach (var registration in registrations)
        {
            if (getNotificationType(registration) == targetGenericType)
            {
                cached.Add(registration);
            }
        }
        return cached;
    }
}

