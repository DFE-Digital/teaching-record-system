namespace TeachingRecordSystem.TestCommon;

public static class EnumerableExtensions
{
    public static IEnumerable<T> RandomSelection<T>(this IEnumerable<T> items, int count) =>
        items.OrderBy(_ => Guid.NewGuid()).Take(count);

    public static T RandomOne<T>(this IEnumerable<T> items) =>
        items.OrderBy(_ => Guid.NewGuid()).First();

    public static T RandomOneExcept<T>(this IEnumerable<T> values, Predicate<T> exclude)
    {
        var options = values;

        if (exclude is not null)
        {
            options = options.Where(o => !exclude(o));
        }

        return options.RandomOne();
    }
}
