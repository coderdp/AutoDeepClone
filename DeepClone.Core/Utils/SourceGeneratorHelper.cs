namespace AutoDeepClone.Core.Utils;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

public static class SourceGeneratorHelper
{
    private static readonly Dictionary<SpecialType, Type> PrimitiveSpecialTypeToCLRTypeMap = new()
    {
        { SpecialType.System_Boolean, typeof(bool) },
        { SpecialType.System_Char, typeof(char) },
        { SpecialType.System_SByte, typeof(sbyte) },
        { SpecialType.System_Byte, typeof(byte) },
        { SpecialType.System_Int16, typeof(short) },
        { SpecialType.System_UInt16, typeof(ushort) },
        { SpecialType.System_Int32, typeof(int) },
        { SpecialType.System_UInt32, typeof(uint) },
        { SpecialType.System_Int64, typeof(long) },
        { SpecialType.System_UInt64, typeof(ulong) },
        { SpecialType.System_Single, typeof(float) },
        { SpecialType.System_Double, typeof(double) },
        { SpecialType.System_Decimal, typeof(decimal) },
        { SpecialType.System_String, typeof(string) }
    };

    public static bool IsPrimitiveClrType(this SpecialType specialType)
    {
        var r = PrimitiveSpecialTypeToCLRTypeMap.Keys.Contains(specialType);

        return r;
    }

    public static bool IsPrimitive(this ITypeSymbol typeSymbol)
        => typeSymbol != null && typeSymbol.SpecialType.IsPrimitiveClrType();
}
