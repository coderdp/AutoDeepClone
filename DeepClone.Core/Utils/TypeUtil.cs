namespace AutoDeepClone.Core.Utils;

using Microsoft.CodeAnalysis;

public static class TypeUtil
{
    public static bool IsKnownBasicCSharpType(this ITypeSymbol typeSymbol)
    {
        var r = typeSymbol.IsPrimitive()
                || typeSymbol.SpecialType == SpecialType.System_DateTime
                || typeSymbol.TypeKind == TypeKind.Enum;

        if (!r)
        {
            var type = typeSymbol.TryGetUnderlyingNullableTypeOrSelf();
        }

        return r;
    }

    public static ITypeSymbol TryGetUnderlyingNullableTypeOrSelf(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T && namedTypeSymbol.TypeArguments.Length == 1)
            {
                return namedTypeSymbol.TypeArguments[0];
            }
        }

        return typeSymbol;
    }
}
