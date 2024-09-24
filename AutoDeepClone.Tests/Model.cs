namespace AutoDeepClone.Tests;

using AutoDeepClone.Core;

[DeepClone]
public partial class BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime? CreateTime { get; set; }
    public FileAccess FileAccess { get; set; }
    public List<string> StringListValue { get; set; }
    public List<int?> IntListValue { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public int[] IntArrayValue { get; set; }
    public int?[] NullableIntArrayValue { get; set; }
    public List<DateTime?> Times { get; set; }
}

[DeepClone]
public partial class ReferenceModel
{
    public BaseEntity BaseEntity { get; set; }
    public BaseEntity[] ObjectArray { get; set; }
    public List<BaseEntity> BaseEntities { get; set; }
    public Dictionary<string, string> Dict { get; set; }
    public Dictionary<string, BaseEntity> BaseEntityDict { get; set; }
}