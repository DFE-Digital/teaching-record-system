using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class ExecuteOnCommitBackgroundJobScheduler(IServiceProvider serviceProvider) : IBackgroundJobScheduler
{
    private readonly ConcurrentDictionary<string, Task> _jobsById = new();

    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> expression) where T : notnull
    {
        var transaction = Transaction.Current;

        if (transaction is null)
        {
            throw new InvalidOperationException("No current transaction.");
        }

        var jobId = Guid.NewGuid().ToString();
        var transactionCompleted = new TaskCompletionSource();

        async Task ScheduleAsync()
        {
            await transactionCompleted.Task;

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<T>();
            var task = expression.Compile()(service);
            await task;
        }

        var jobTask = ScheduleAsync();

        _jobsById.TryAdd(jobId, jobTask);

        transaction.TransactionCompleted += (_, t) =>
        {
            if (t.Transaction?.TransactionInformation.Status == TransactionStatus.Committed)
            {
                transactionCompleted.SetResult();
            }
            else
            {
                transactionCompleted.SetException(new Exception("Transaction failed."));
            }

            jobTask.Wait();
        };

        return Task.FromResult(jobId);
    }

    public Task<string> ContinueJobWithAsync<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull
    {
        if (!_jobsById.TryGetValue(parentId, out var parent))
        {
            throw new ArgumentException($"Cannot find job with ID: '{parentId}')");
        }

        var jobId = Guid.NewGuid().ToString();
        _jobsById.TryAdd(jobId, ScheduleAsync());
        return Task.FromResult(jobId);

        async Task ScheduleAsync()
        {
            await parent;

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<T>();
            var task = expression.Compile()(service);
            await task;
        }
    }

    public async Task WaitForJobToCompleteAsync(string jobId, CancellationToken cancellationToken)
    {
        if (!_jobsById.TryGetValue(jobId, out var job))
        {
            throw new ArgumentException($"Cannot find job with ID: '{jobId}')");
        }

        await job.WaitAsync(cancellationToken);
    }
}

