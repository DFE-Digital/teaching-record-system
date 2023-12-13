namespace TeachingRecordSystem.TestCommon;

public static class EnumerableExtensions
{
    public static T RandomOne<T>(this IEnumerable<T> items) =>
        items.OrderBy(_ => Guid.NewGuid()).First();
}
