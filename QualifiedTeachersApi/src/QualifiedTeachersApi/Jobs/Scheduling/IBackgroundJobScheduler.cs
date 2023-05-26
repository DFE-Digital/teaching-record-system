using System.Linq.Expressions;

namespace QualifiedTeachersApi.Jobs.Scheduling;

public interface IBackgroundJobScheduler
{
    Task Enqueue<T>(Expression<Func<T, Task>> expression) where T : notnull;
}
