namespace AutoDeepClone.Tests;

public class MainTest
{
    private BaseEntity InitBaseEntity(int? id = 1)
    {
        var model = new BaseEntity()
        {
            Id = id.Value,
            Name = "test",
            CreateTime = DateTime.Now,
            DayOfWeek = DayOfWeek.Monday,
            FileAccess = FileAccess.Write,
            IntArrayValue = new[] { 1, 2, 3, },
            IntListValue = new List<int?> { 1, 2, 3, },
            NullableIntArrayValue = new int?[] { 1, 2, },
            StringListValue = new List<string> { "1", "2" },
            Times = new List<DateTime?> { DateTime.Now, }
        };

        return model;
    }

    [Fact]
    public void TestDeepCloneSimple()
    {
        var model = InitBaseEntity();
        var deepCloneModel = model.DeepClone();
        AssertBaseEntity(model, deepCloneModel);
    }

    private static bool AssertBaseEntity(BaseEntity left, BaseEntity right)
    {
        Assert.Equal(left.Id, right.Id);
        Assert.Equal(left.Name, right.Name);
        Assert.Equal(left.CreateTime, right.CreateTime);
        Assert.Equal(left.FileAccess, right.FileAccess);
        Assert.Equal(left.DayOfWeek, right.DayOfWeek);

        AssertBaseEntityList(left.StringListValue, right.StringListValue);
        AssertBaseEntityList(left.IntListValue, right.IntListValue);
        AssertBaseEntityArray(left.IntArrayValue, right.IntArrayValue);
        AssertBaseEntityArray(left.NullableIntArrayValue, right.NullableIntArrayValue);
        AssertBaseEntityList(left.Times, right.Times);

        return true;
    }

    private static void AssertBaseEntityList<T>(List<T> left, List<T> right)
    {
        var flag = left.TrueForAll(o => right.Contains(o)) && right.TrueForAll(o => left.Contains(o));
        Assert.True(flag);
    }

    private static void AssertBaseEntityArray<T>(T[] left, T[] right)
    {
        var flag = left.SequenceEqual(right);
        Assert.True(flag);
    }

    [Fact]
    public void TestReferenceModelDeepClone()
    {
        var model = new ReferenceModel()
        {
            BaseEntity = InitBaseEntity(1),
            BaseEntities = new List<BaseEntity> { InitBaseEntity(2), InitBaseEntity(3) },
            BaseEntityDict = new Dictionary<string, BaseEntity> { { "1", InitBaseEntity(4) } },
            Dict = new Dictionary<string, string>
            {
                { "1","1" },
                { "2","2" },
            },
            ObjectArray = new BaseEntity[] { InitBaseEntity(5), InitBaseEntity(6) }
        };
        var deepCloneModel = model.DeepClone();

        AssertReferenceModel(model, deepCloneModel);
    }

    private static void AssertReferenceModel(ReferenceModel left, ReferenceModel right)
    {
        AssertBaseEntity(left.BaseEntity, right.BaseEntity);

        for (int i = 0; i < left.ObjectArray.Length; i++)
        {
            var leftItem = left.ObjectArray[i];
            var rightItem = right.ObjectArray[i];
            AssertBaseEntity(leftItem, rightItem);
        }

        AssertBaseEntities(left.BaseEntities, right.BaseEntities);
        AssertBaseEntities(right.BaseEntities, left.BaseEntities);

        Func<string, string, bool> func = (x, y) => x == y;
        AssertDictionary(left.Dict, right.Dict, func);
        AssertDictionary(left.BaseEntityDict, right.BaseEntityDict, AssertBaseEntity);
    }

    private static void AssertBaseEntities(List<BaseEntity> left, List<BaseEntity> right)
    {
        left.TrueForAll(leftItem =>
        {
            var rightItem = right.FirstOrDefault(r => leftItem.Id == r.Id);
            Assert.NotNull(rightItem);
            AssertBaseEntity(leftItem, rightItem);
            return true;
        });
    }

    private static void AssertDictionary<K, V>(Dictionary<K, V> left, Dictionary<K, V> right, Func<V, V, bool> func)
    {
        foreach (var leftItem in left)
        {
            var exist = right.TryGetValue(leftItem.Key, out V value);
            Assert.True(exist);
            Assert.NotNull(value);

            var result = func(leftItem.Value, value);
            Assert.True(result);
        }
    }
}
