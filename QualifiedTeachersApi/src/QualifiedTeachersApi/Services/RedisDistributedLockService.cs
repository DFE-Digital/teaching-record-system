using System;
using RedLockNet.SERedis;

namespace QualifiedTeachersApi.Services
{
    public class RedisDistributedLockService : IDistributedLockService
    {
        private static readonly TimeSpan _lifetime = TimeSpan.FromHours(1);
        private static readonly TimeSpan _retryPeriod = TimeSpan.FromSeconds(2);

        private readonly RedLockFactory _redLockFactory;

        public RedisDistributedLockService(RedLockFactory redLockFactory)
        {
            _redLockFactory = redLockFactory;
        }

        public async System.Threading.Tasks.Task<IAsyncDisposable> AcquireLock(string key, TimeSpan timeout)
        {
            var @lock = await _redLockFactory.CreateLockAsync(key, _lifetime, waitTime: timeout, retryTime: _retryPeriod);

            if (!@lock.IsAcquired)
            {
                await @lock.DisposeAsync();
                throw new Exception($"Failed to acquire lock for key: '{key}' after {timeout.TotalSeconds}s.");
            }

            return @lock;
        }
    }
}
