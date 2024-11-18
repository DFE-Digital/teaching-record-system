namespace TeachingRecordSystem.Core.Dqt;

public interface ICrmQueryDispatcher
{
    Task<TResult> ExecuteQueryAsync<TResult>(ICrmQuery<TResult> query);
    IAsyncEnumerable<TResult> ExecuteQueryAsync<TResult>(IEnumerableCrmQuery<TResult> query, CancellationToken cancellationToken = default);
    CrmTransactionScope CreateTransactionRequestBuilder();
}
