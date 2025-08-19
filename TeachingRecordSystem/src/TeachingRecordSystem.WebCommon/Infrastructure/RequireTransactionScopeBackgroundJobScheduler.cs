using System.Linq.Expressions;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.WebCommon.Infrastructure;

public class RequireTransactionScopeBackgroundJobScheduler(IBackgroundJobScheduler innerScheduler) : IBackgroundJobScheduler
{
    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> expression) where T : notnull
    {
        RequireTransactionScope();
        return innerScheduler.EnqueueAsync(expression);
    }

    public Task<string> ContinueJobWithAsync<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull
    {
        RequireTransactionScope();
        return innerScheduler.ContinueJobWithAsync(parentId, expression);
    }

    public Task WaitForJobToCompleteAsync(string jobId, CancellationToken cancellationToken) =>
        innerScheduler.WaitForJobToCompleteAsync(jobId, cancellationToken);

    private void RequireTransactionScope()
    {
        if (System.Transactions.Transaction.Current is null)
        {
            throw new InvalidOperationException("A transaction scope is required.");
        }
    }
}
