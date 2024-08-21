namespace TeachingRecordSystem.Core.Dqt;

public interface ICrmQueryDispatcher
{
    Task<TResult> ExecuteQuery<TResult>(ICrmQuery<TResult> query);
    IAsyncEnumerable<TResult> ExecuteQuery<TResult>(IEnumerableCrmQuery<TResult> query, CancellationToken cancellationToken = default);
}
