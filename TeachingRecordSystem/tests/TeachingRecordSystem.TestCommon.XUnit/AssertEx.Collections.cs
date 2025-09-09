using Xunit;

namespace TeachingRecordSystem.TestCommon;

public static partial class AssertEx
{
    public static void ContainsAll<T>(IEnumerable<T> expected, IEnumerable<T> collection)
    {
        foreach (var item in expected)
        {
            Assert.Contains(item, collection);
        }
    }

    public static void DoesNotContainAny<T>(IEnumerable<T> expected, IEnumerable<T> collection)
    {
        foreach (var item in expected)
        {
            Assert.DoesNotContain(item, collection);
        }
    }
}
