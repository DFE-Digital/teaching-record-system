using System;

namespace QualifiedTeachersApi.Services;

public interface IDistributedLockService
{
    System.Threading.Tasks.Task<IAsyncDisposable> AcquireLock(string key, TimeSpan timeout);
}
