namespace AutoDeepClone.Core.Utils;

internal static class SourceTextHelper
{
    public static string UseCRLFLineEnding(this string text)
    {
        if (text == null)
        {
            return text;
        }

        var r = text.Replace("\r\n", "\n").Replace("\n", "\r\n");

        return r;
    }
}
