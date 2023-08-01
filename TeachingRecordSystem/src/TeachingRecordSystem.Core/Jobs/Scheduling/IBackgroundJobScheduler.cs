using System.Linq.Expressions;

namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public interface IBackgroundJobScheduler
{
    Task Enqueue<T>(Expression<Func<T, Task>> expression) where T : notnull;
}
