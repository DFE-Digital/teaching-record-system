using System.Linq.Expressions;

namespace TeachingRecordSystem.SupportUi;

public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}

public static class SortDirectionQueryableExtensions
{
    public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
        this IQueryable<TSource> source,
        SortDirection direction,
        Expression<Func<TSource, TKey>> keySelector)
    {
        return direction == SortDirection.Ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
    }

    public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
        this IOrderedQueryable<TSource> source,
        SortDirection direction,
        Expression<Func<TSource, TKey>> keySelector)
    {
        return direction == SortDirection.Ascending ? source.ThenBy(keySelector) : source.ThenByDescending(keySelector);
    }
}
