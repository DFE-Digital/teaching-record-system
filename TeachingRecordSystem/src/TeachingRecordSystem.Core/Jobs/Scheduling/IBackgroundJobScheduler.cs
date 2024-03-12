using System.Linq.Expressions;

namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public interface IBackgroundJobScheduler
{
    Task<string> Enqueue<T>(Expression<Func<T, Task>> expression) where T : notnull;

    Task<string> ContinueJobWith<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull;
}
