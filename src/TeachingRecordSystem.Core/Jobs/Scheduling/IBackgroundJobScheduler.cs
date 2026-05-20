using System.Linq.Expressions;

namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public interface IBackgroundJobScheduler
{
    Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> expression) where T : notnull;

    Task<string> ContinueJobWithAsync<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull;

    Task WaitForJobToCompleteAsync(string jobId, CancellationToken cancellationToken);
}
