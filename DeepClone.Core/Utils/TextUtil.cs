namespace AutoDeepClone.Core.Utils;

internal static class TextUtil
{
    public static string OpSafe(this string text)
    {
        return text ?? string.Empty;
    }

    public static string TrimSafe(this string text)
    {
        return text.OpSafe().Trim();
    }
}
