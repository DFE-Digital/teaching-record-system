using Xunit;

namespace TeachingRecordSystem.TestCommon;

#pragma warning disable CA1711
public static partial class AssertEx
#pragma warning restore CA1711
{
    public static void ContainsAll<T>(IEnumerable<T> expected, IReadOnlyCollection<T> collection)
    {
        foreach (var item in expected)
        {
            Assert.Contains(item, collection);
        }
    }

    public static void DoesNotContainAny<T>(IEnumerable<T> expected, IReadOnlyCollection<T> collection)
    {
        foreach (var item in expected)
        {
            Assert.DoesNotContain(item, collection);
        }
    }
}
