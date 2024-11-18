using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.TestCommon;

public class CrmQueryDispatcherSpy
{
    private readonly List<(object Query, object? Result)> _queries = [];

    public IReadOnlyCollection<(TQuery Query, TResult Result)> GetAllQueries<TQuery, TResult>()
        where TQuery : ICrmQuery<TResult>
    {
        var queryType = typeof(TQuery);

        return _queries.Where(q => q.Query.GetType() == queryType)
            .Select(q => (Query: (TQuery)q.Query, Result: (TResult)q.Result!))
            .AsReadOnly();
    }

    public (TQuery Query, TResult Result) GetSingleQuery<TQuery, TResult>()
        where TQuery : ICrmQuery<TResult>
    {
        var queries = GetAllQueries<TQuery, TResult>();

        if (queries.Count == 0)
        {
            throw new InvalidOperationException($"No {typeof(TQuery).Name} queries have been executed.");
        }
        else if (queries.Count > 1)
        {
            throw new InvalidOperationException($"Multiple {typeof(TQuery).Name} queries have been executed.");
        }

        return queries.Single();
    }

    internal void RegisterQuery(object query, object? result)
    {
        _queries.Add((query, result));
    }
}

public class CrmQueryDispatcherDecorator(ICrmQueryDispatcher innerDispatcher, CrmQueryDispatcherSpy spy) : ICrmQueryDispatcher
{
    public async Task<TResult> ExecuteQueryAsync<TResult>(ICrmQuery<TResult> query)
    {
        var result = await innerDispatcher.ExecuteQueryAsync(query);
        spy.RegisterQuery(query, result);
        return result;
    }

    public IAsyncEnumerable<TResult> ExecuteQueryAsync<TResult>(IEnumerableCrmQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return innerDispatcher.ExecuteQueryAsync(query, cancellationToken);
    }

    public CrmTransactionScope CreateTransactionRequestBuilder()
    {
        return innerDispatcher.CreateTransactionRequestBuilder();
    }
}
