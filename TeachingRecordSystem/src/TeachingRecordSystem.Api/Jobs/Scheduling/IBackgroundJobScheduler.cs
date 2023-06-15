using System.Linq.Expressions;

namespace TeachingRecordSystem.Api.Jobs.Scheduling;

public interface IBackgroundJobScheduler
{
    Task Enqueue<T>(Expression<Func<T, Task>> expression) where T : notnull;
}
