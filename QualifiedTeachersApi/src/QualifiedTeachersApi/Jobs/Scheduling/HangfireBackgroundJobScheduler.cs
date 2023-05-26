using System.Linq.Expressions;
using Hangfire;

namespace QualifiedTeachersApi.Jobs.Scheduling;

public class HangfireBackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IBackgroundJobClient _jobClient;

    public HangfireBackgroundJobScheduler(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient;
    }

    public Task Enqueue<T>(Expression<Func<T, Task>> expression) where T : notnull
    {
        _jobClient.Enqueue(expression);
        return Task.CompletedTask;
    }
}
