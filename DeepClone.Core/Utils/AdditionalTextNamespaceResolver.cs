namespace AutoDeepClone.Core.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using System;
using System.IO;

internal static class AdditionalTextNamespaceResolver
{
    public static string ResolveDirectory(AdditionalText file, AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider)
    {
        if (!analyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace))
        {
            return null;
        }

        if (!analyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.ProjectDir", out var projectDir))
        {
            return rootNamespace;
        }

        var fromPath = EnsurePathEndsWithDirectorySeparator(projectDir);
        var toPath = EnsurePathEndsWithDirectorySeparator(Path.GetDirectoryName(file.Path));
        var relativePath = GetRelativePath(fromPath, toPath);

        var namespaceName = $"{rootNamespace}.{relativePath.Replace(Path.DirectorySeparatorChar, '.')}".TrimEnd('.');

        return namespaceName;
    }

    private static string EnsurePathEndsWithDirectorySeparator(string path)
        => path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

    private static string GetRelativePath(string fromPath, string toPath)
    {
        var relativeUri = new Uri(fromPath).MakeRelativeUri(new(toPath));

        var r = Uri.UnescapeDataString(relativeUri.ToString())
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        return r;
    }
}
