using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace DqtApi
{
    public static class MemoryCacheExtensions
    {
        public static async Task<TItem> GetOrCreateUnlessNullAsync<TItem>(this IMemoryCache cache, object key, Func<Task<TItem>> factory)
            where TItem : class
        {
            if (!cache.TryGetValue(key, out object result))
            {
                result = await factory().ConfigureAwait(false);

                if (result is not null)
                {
                    using ICacheEntry entry = cache.CreateEntry(key);
                    entry.Value = result;
                }
            }

            return (TItem)result;
        }
    }
}
