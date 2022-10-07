using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DqtApi.Services
{
    public class LocalDistributedLockService : IDistributedLockService
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public async Task<IAsyncDisposable> AcquireLock(string key, TimeSpan timeout)
        {
            var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            if (!await sem.WaitAsync(timeout))
            {
                throw new Exception($"Failed to acquire lock for key: '{key}' after {timeout.TotalSeconds}s.");
            }
            return new LockDisposableWrapper(sem);
        }

        private sealed class LockDisposableWrapper : IAsyncDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public LockDisposableWrapper(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public ValueTask DisposeAsync()
            {
                _semaphore.Release();
                return default;
            }
        }
    }
}
