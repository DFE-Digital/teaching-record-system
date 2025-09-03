namespace TeachingRecordSystem.TestCommon;

public static class EnumerableExtensions
{
    public static IEnumerable<T> RandomSelection<T>(this IEnumerable<T> items, int count) =>
        items.OrderBy(_ => Guid.NewGuid()).Take(count);

    public static T RandomOne<T>(this IEnumerable<T> items) =>
        items.OrderBy(_ => Guid.NewGuid()).First();

    public static T RandomOneExcept<T>(this IEnumerable<T> values, Predicate<T> exclude) =>
        values.Where(o => !exclude(o)).RandomOne();
}
