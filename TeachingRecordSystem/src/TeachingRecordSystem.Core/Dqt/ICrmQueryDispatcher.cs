namespace TeachingRecordSystem.Core.Dqt;

public interface ICrmQueryDispatcher
{
    Task<TResult> ExecuteQuery<TResult>(ICrmQuery<TResult> query);
}
