namespace AutoDeepClone.Core.Utils;

using Microsoft.CodeAnalysis;

using System.Collections.Generic;

public static class ITypeSymbolHelpers
{
    private static readonly Dictionary<Accessibility, string> AccessibilityMap = new Dictionary<Accessibility, string>
    {
        { Accessibility.Public, "public" },
        { Accessibility.Internal, "internal" },
        { Accessibility.Private, "private" },
        { Accessibility.Protected, "protected" },
        { Accessibility.ProtectedAndInternal, "protected internal" },
    };

    public static bool IsNullableType(this ITypeSymbol typeOpt)
    {
        if (typeOpt == null)
        {
            return false;
        }

        return typeOpt.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    public static bool IsNullableOfBoolean(this ITypeSymbol type)
    {
        return IsNullableType(type) && IsBooleanType(GetNullableUnderlyingType(type));
    }

    public static ITypeSymbol GetNullableUnderlyingType(this ITypeSymbol type)
    {
        return ((INamedTypeSymbol)type).TypeArguments[0];
    }

    public static bool IsBooleanType(this ITypeSymbol type)
    {
        return type?.SpecialType == SpecialType.System_Boolean;
    }

    public static bool IsObjectType(this ITypeSymbol type)
    {
        return type?.SpecialType == SpecialType.System_Object;
    }

    public static string GetAccessibilityCode(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null)
        {
            return null;
        }

        var accessibility = typeSymbol.DeclaredAccessibility;
        AccessibilityMap.TryGetValue(accessibility, out var r);

        return r;
    }
}
