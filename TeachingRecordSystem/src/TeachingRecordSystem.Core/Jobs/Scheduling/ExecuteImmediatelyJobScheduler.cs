using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public class ExecuteImmediatelyJobScheduler : IBackgroundJobScheduler
{
    private readonly IServiceProvider _serviceProvider;

    public ExecuteImmediatelyJobScheduler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<string> Enqueue<T>(Expression<Func<T, Task>> expression) where T : notnull
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        var task = expression.Compile()(service);
        await task;
        return Guid.NewGuid().ToString();
    }

    public Task<string> ContinueJobWith<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull
    {
        return Enqueue(expression);
    }
}
