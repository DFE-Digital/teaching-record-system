using System.Linq.Expressions;

namespace TeachingRecordSystem.SupportUi.Pages.Common;

public class SingleValueFilter<T> : Filter<T>
{
    public SingleValueFilter(
        string name,
        string displayName,
        IQueryCollection currentRequestQueryString,
        Func<string, Expression<Func<T, bool>>> buildFilterExpressionFromFilterValue)
        : base(name, displayName)
    {
        Value = currentRequestQueryString[name];

        _filterExpression = !string.IsNullOrWhiteSpace(Value)
            ? buildFilterExpressionFromFilterValue(Value)
            : x => true;
    }

    private readonly Expression<Func<T, bool>> _filterExpression;

    public string? Value { get; }

    public override IQueryable<T> Apply(IQueryable<T> query)
    {
        return query.Where(_filterExpression);
    }
}
