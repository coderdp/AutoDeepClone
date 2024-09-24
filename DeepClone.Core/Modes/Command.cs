namespace AutoDeepClone.Core.Modes;

using AutoDeepClone.Core.Utils;

using System;

internal record GenerateComparerCommand
{
    public ItemEqualityCollection<GeneratorPropertyDescription> Properties { get; set; } = new();
    public GeneratorClassDescription TargetClassDescription { get; set; }
    public GeneratorClassDescription HostingClassDescription { get; set; }

    public ItemEqualityCollection<string> UsingNamespaces { get; set; } = new();
}

internal record GeneratorPropertyDescription
{
    public bool ShouldGenerate => !Ignore
                                  && ValueType != GeneratorPropertyValueType.NotSupported
                                  && ValueType != GeneratorPropertyValueType.Unknown;

    public GeneratorPropertyValueType ValueType { get; set; }
    public bool Ignore { get; set; }
    public string Name { get; set; }
    public string FullTypeName { get; set; }

    public GeneratorPropertyValueMapType MapType { get; set; }

    public void FillWith(params Action[] actions)
    {
        if (actions == null)
        {
            return;
        }

        foreach (var action in actions)
        {
            if (ValueType != GeneratorPropertyValueType.Unknown)
            {
                break;
            }

            action();
        }
    }
}

internal enum GeneratorPropertyValueType
{
    Unknown = 0,
    NotSupported,
    Basic,
    Array,
    List,
    Dictionary,
    Object,
}

internal enum GeneratorPropertyValueMapType
{
    Simple = 0,
    DeepClone,
    Select,
    SelectDeepClone
}

internal record GeneratorClassDescription
{
    public string ClassName { get; init; }
    public string Namespace { get; init; }
    public string FullClassName { get; init; }

    public string Accessibility { get; set; }

    public GeneratorClassDescription ContainingClass { get; init; }

    public string GetDisplayClassName()
    {
        var r = ContainingClass == null ? ClassName : $"{ContainingClass.GetDisplayClassName()}.{ClassName}";
        return r;
    }
}
