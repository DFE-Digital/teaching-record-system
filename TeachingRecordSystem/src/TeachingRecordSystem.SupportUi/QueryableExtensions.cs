using System.Collections;
using System.Linq.Expressions;

namespace TeachingRecordSystem.SupportUi;

public static class QueryableExtensions
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

    public static Task<ResultPage<T>> GetPageAsync<T>(
        this IQueryable<T> source,
        int? currentPage,
        int itemsPerPage,
        int? total = null)
    {
        return ResultPage.CreateFromQueryAsync(source, currentPage, itemsPerPage, total);
    }
}

public class ResultPage<T>(IReadOnlyCollection<T> items, int currentPage, int totalItems, int itemsPerPage) : IReadOnlyCollection<T>
{
    public int LastPage => ResultPage.GetLastPage(totalItems, itemsPerPage);

    public int CurrentPage => currentPage;

    public int TotalItems => totalItems;

    public int ItemsPerPage => itemsPerPage;

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => items.Count;
}

public static class ResultPage
{
    public static int GetLastPage(int totalItems, int itemsPerPage) =>
        Math.Max(1, (int)Math.Ceiling(totalItems / (decimal)itemsPerPage));

    public static async Task<ResultPage<T>> CreateFromQueryAsync<T>(
        this IQueryable<T> source,
        int? currentPage,
        int itemsPerPage,
        int? total = null)
    {
        total ??= await source.CountAsync();

        var lastPage = GetLastPage(total.Value, itemsPerPage);
        var page = Math.Clamp(currentPage ?? 1, 1, lastPage);

        var items = await source
            .Skip((page - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToArrayAsync();

        return new ResultPage<T>(items.AsReadOnly(), page, total.Value, itemsPerPage);
    }
}
