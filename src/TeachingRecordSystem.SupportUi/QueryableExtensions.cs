using System.Collections;
using System.Linq.Expressions;

namespace TeachingRecordSystem.SupportUi;

public static class QueryableExtensions
{
    public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TKey>> keySelector,
        SortDirection direction)
    {
        return direction == SortDirection.Ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
    }

    public static IOrderedQueryable<TSource> OrderBy<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, string>> keySelector,
        StringComparer comparer,
        SortDirection direction)
    {
        return direction == SortDirection.Ascending ? source.OrderBy(keySelector, comparer) : source.OrderByDescending(keySelector, comparer);
    }

    public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
        this IOrderedQueryable<TSource> source,
        Expression<Func<TSource, TKey>> keySelector,
        SortDirection direction)
    {
        return direction == SortDirection.Ascending ? source.ThenBy(keySelector) : source.ThenByDescending(keySelector);
    }

    public static IOrderedQueryable<TSource> ThenBy<TSource>(
        this IOrderedQueryable<TSource> source,
        Expression<Func<TSource, string>> keySelector,
        StringComparer comparer,
        SortDirection direction)
    {
        return direction == SortDirection.Ascending ? source.ThenBy(keySelector, comparer) : source.ThenByDescending(keySelector, comparer);
    }

    public static async Task<ResultPage<T>> GetPageAsync<T>(
        this IQueryable<T> source,
        int? currentPage,
        int itemsPerPage,
        int totalItemCount)
    {
        var page = ResultPage.ResolveCurrentPage(currentPage, itemsPerPage, totalItemCount);

        var items = await source
            .Skip((page - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToArrayAsync();

        return new ResultPage<T>(items.AsReadOnly(), page, itemsPerPage, totalItemCount);
    }

    public static ResultPage<T> GetPage<T>(
        this IEnumerable<T> source,
        int? currentPage,
        int itemsPerPage,
        int totalItemCount)
    {
        var page = ResultPage.ResolveCurrentPage(currentPage, itemsPerPage, totalItemCount);

        var items = source
            .Skip((page - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToArray();

        return new ResultPage<T>(items.AsReadOnly(), page, itemsPerPage, totalItemCount);
    }
}

public class ResultPage<T>(IReadOnlyCollection<T> items, int currentPage, int itemsPerPage, int totalItemCount)
    : ResultPage(currentPage, itemsPerPage, totalItemCount), IReadOnlyCollection<T>
{
    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => items.Count;
}

public class ResultPage(int currentPage, int itemsPerPage, int totalItemCount)
{
    public int CurrentPage => currentPage;
    public int LastPage => GetLastPage(itemsPerPage, totalItemCount);
    public int ItemsPerPage => itemsPerPage;
    public int TotalItemCount => totalItemCount;

    public static int ResolveCurrentPage(int? currentPage, int itemsPerPage, int totalItems)
    {
        var lastPage = GetLastPage(itemsPerPage, totalItems);

        return Math.Clamp(currentPage ?? 1, 1, lastPage);
    }

    private static int GetLastPage(int itemsPerPage, int totalItems) =>
        Math.Max(1, (int)Math.Ceiling(totalItems / (decimal)itemsPerPage));
}
