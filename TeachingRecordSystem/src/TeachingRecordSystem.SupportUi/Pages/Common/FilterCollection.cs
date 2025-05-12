using System.Collections;
using System.Linq.Expressions;

namespace TeachingRecordSystem.SupportUi.Pages.Common;

public class FilterCollection<T> : IReadOnlyCollection<Filter<T>>
{
    public FilterCollection(IReadOnlyCollection<Filter<T>> filters)
    {
#pragma warning disable CA2021 // CA2021 incorrectly warns that MultiValueFilter<T> is incompatible with Filter<T> - see https://github.com/dotnet/roslyn-analyzers/issues/7357
        _multiValueFilters = filters.OfType<MultiValueFilter<T>>().ToList();
#pragma warning restore CA2021 // CA2021 incorrectly warns that MultiValueFilter<T> is incompatible with Filter<T> - see https://github.com/dotnet/roslyn-analyzers/issues/7357

        _groupByExpressionForFilterValueCounts = BuildGroupByExpressionForFilterValueCounts(_multiValueFilters);
        _filters = filters;
    }

    private readonly IReadOnlyCollection<Filter<T>> _filters;
    private readonly IReadOnlyList<MultiValueFilter<T>> _multiValueFilters;
    private readonly Expression<Func<T, string?[]>> _groupByExpressionForFilterValueCounts;

    public int Count => _filters.Count;
    public IEnumerator<Filter<T>> GetEnumerator() => _filters.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        var filteredQuery = query;

        foreach (var filter in this)
        {
            filteredQuery = filter.Apply(filteredQuery);
        }

        return filteredQuery;
    }

    public async Task<int> CalculateFilterCountsAsync(IQueryable<T> query)
    {
        // Groups entities by a string array of filter values:
        // First element in the array is a value of the first multi-value filter, second element is a value of the second multi-value filter etc.
        // E.g. for 3 filters with possible values filter 1: ['A', 'B', 'C'], filter 2: ['I', 'J'], filter 3: ['X', 'Y', 'Z']
        // this will produce a key with values ['A', 'I', 'X'], ['B', 'I', 'X'], ['C', 'I', 'X'], ['A', 'J', 'X'], ['B', 'J', 'X'] etc.
        var counts = await query
            .GroupBy(_groupByExpressionForFilterValueCounts)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToArrayAsync();

        // The total count of all entities is the sum of all the partitioned counts.
        // Null/empty values are also captured by the group by expression so the total count includes these values
        // even if they are not explicitly represented in the available filter values.
        var totalEntities = counts.Sum(r => r.Count);

        // Extract the counts for each filter by the corresponding index of the group key
        for (var i = 0; i < _multiValueFilters.Count; i++)
        {
            var filter = _multiValueFilters[i];
            var countsForFilter = counts.Select(c => new FilterValueCount { FilterValue = c.Key[i], Count = c.Count });

            filter.UpdateFilterValueCounts(countsForFilter);
        }

        return totalEntities;
    }

    private static Expression<Func<T, string?[]>> BuildGroupByExpressionForFilterValueCounts(IEnumerable<MultiValueFilter<T>> multiValueFilters)
    {
        // construct groupBy lambda expression equivalent to:
        // x => new string[] { ..multiValueFilters.Select(f => f.ValueExpression(x)) }
        var parameter = Expression.Parameter(typeof(T), "x");
        var values = multiValueFilters.Select(f => Expression.Invoke(f.ValueExpression, parameter));
        var expression = Expression.Lambda<Func<T, string?[]>>(Expression.NewArrayInit(typeof(string), values), parameter);

        return expression;
    }
}
