using System.Linq.Expressions;
using System.Transactions;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.TestCommon;

public class TestBackgroundJobScheduler(IServiceProvider serviceProvider) : IBackgroundJobScheduler
{
    private readonly ExecuteImmediatelyJobScheduler _executeImmediatelyJobScheduler = new(serviceProvider);
    private readonly ExecuteOnCommitBackgroundJobScheduler _executeOnCommitBackgroundJobScheduler = new(serviceProvider);

    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> expression) where T : notnull
    {
        if (Transaction.Current is not null)
        {
            return _executeOnCommitBackgroundJobScheduler.EnqueueAsync(expression);
        }

        return _executeImmediatelyJobScheduler.EnqueueAsync(expression);
    }

    public Task<string> ContinueJobWithAsync<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull
    {
        if (Transaction.Current is not null)
        {
            return _executeOnCommitBackgroundJobScheduler.ContinueJobWithAsync(parentId, expression);
        }

        return _executeImmediatelyJobScheduler.ContinueJobWithAsync(parentId, expression);
    }

    public Task WaitForJobToCompleteAsync(string jobId, CancellationToken cancellationToken)
    {
        if (Transaction.Current is not null)
        {
            return _executeOnCommitBackgroundJobScheduler.WaitForJobToCompleteAsync(jobId, cancellationToken);
        }

        return _executeImmediatelyJobScheduler.WaitForJobToCompleteAsync(jobId, cancellationToken);
    }
}
