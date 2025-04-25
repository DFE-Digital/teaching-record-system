using System.Linq.Expressions;

namespace TeachingRecordSystem.SupportUi.Pages.Common;

public class MultiValueFilter<T> : Filter<T>
{
    public MultiValueFilter(
        string name,
        string displayName,
        IQueryCollection currentRequestQueryString,
        Expression<Func<T, string?>> valueExpression,
        IReadOnlyCollection<MultiValueFilterValue> values)
         : base(name, displayName)
    {
        string[]? selectedValues = currentRequestQueryString[name];

        _filterExpression = selectedValues != null && selectedValues.Any(r => !string.IsNullOrWhiteSpace(r))
            ? BuildContainsExpression(selectedValues, valueExpression)
            : x => true;

        ValueExpression = valueExpression;
        Values = values;

        foreach (var value in values)
        {
            value.UpdateSelected(selectedValues);
        }
    }

    private readonly Expression<Func<T, bool>> _filterExpression;

    public Expression<Func<T, string?>> ValueExpression { get; }
    public IReadOnlyCollection<MultiValueFilterValue> Values { get; }

    public override IQueryable<T> Apply(IQueryable<T> query)
    {
        return query.Where(_filterExpression);
    }

    public void UpdateFilterValueCounts(IEnumerable<FilterValueCount> valueCountsForFilter)
    {
        foreach (var value in Values)
        {
            value.UpdateCounts(valueCountsForFilter);
        }
    }

    private static Expression<Func<T, bool>> BuildContainsExpression(string[] selectedValues, Expression<Func<T, string?>> valueExpression)
    {
        // construct lambda expression equivalent to:
        // x => selectedValues.Contains(valueExpression(x))
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression<Func<string[], string?, bool>> contains = (v, x) => v.Contains(x);
        Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(
            Expression.Invoke(contains, Expression.Constant(selectedValues), Expression.Invoke(valueExpression, parameter)),
            parameter);

        return expression;
    }
}
