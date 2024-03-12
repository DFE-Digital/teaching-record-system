using System.Linq.Expressions;
using Hangfire;

namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public class HangfireBackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IBackgroundJobClient _jobClient;

    public HangfireBackgroundJobScheduler(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient;
    }

    public Task<string> Enqueue<T>(Expression<Func<T, Task>> expression) where T : notnull
    {
        var jobId = _jobClient.Enqueue(expression);
        return Task.FromResult(jobId);
    }

    public Task<string> ContinueJobWith<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull
    {
        var jobId = _jobClient.ContinueJobWith(parentId, expression);
        return Task.FromResult(jobId);
    }
}
