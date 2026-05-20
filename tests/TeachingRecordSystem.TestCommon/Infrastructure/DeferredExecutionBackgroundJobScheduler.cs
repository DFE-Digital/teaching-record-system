using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class DeferredExecutionBackgroundJobScheduler(IServiceProvider serviceProvider) : IBackgroundJobScheduler
{
    private readonly List<Func<Task>> _deferredJobs = new();

    public async Task ExecuteDeferredJobsAsync()
    {
        try
        {
            foreach (var job in _deferredJobs)
            {
                await job();
            }
        }
        finally
        {
            _deferredJobs.Clear();
        }
    }

    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> expression) where T : notnull
    {
        _deferredJobs.Add(ExecuteJobAsync);

        return Task.FromResult(Guid.NewGuid().ToString());

        async Task ExecuteJobAsync()
        {
            using var scope = serviceProvider.CreateScope();
            var service = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider);
            var task = expression.Compile()(service);
            await task;
        }
    }

    public Task<string> ContinueJobWithAsync<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull
    {
        throw new NotSupportedException();
    }

    public Task WaitForJobToCompleteAsync(string jobId, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}
