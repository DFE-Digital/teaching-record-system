namespace TeachingRecordSystem.TestCommon;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> items, int count) =>
        items.OrderBy(_ => Guid.NewGuid()).Take(count);

    public static T SingleRandom<T>(this IEnumerable<T> items) =>
        items.OrderBy(_ => Guid.NewGuid()).First();

    public static T SingleRandom<T>(this IEnumerable<T> values, Predicate<T> filter) =>
        values.Where(o => filter(o)).SingleRandom();
}
