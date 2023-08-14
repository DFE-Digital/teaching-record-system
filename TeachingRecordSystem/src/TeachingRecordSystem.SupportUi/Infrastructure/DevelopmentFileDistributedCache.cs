using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace TeachingRecordSystem.SupportUi.Infrastructure;

public class DevelopmentFileDistributedCache : IDistributedCache
{
    private readonly object _gate = new object();

    public byte[]? Get(string key)
    {
        byte[]? data = null;

        WithCacheFile(cacheFile =>
        {
            if (cacheFile.Entries.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired)
                {
                    cacheFile.Entries.Remove(key);
                }
                else
                {
                    entry.LastAccess = DateTimeOffset.UtcNow;
                    data = entry.Data;
                }
            }
        });

        return data;
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        return Task.FromResult(Get(key));
    }

    public void Refresh(string key) => WithCacheFile(cacheFile =>
    {
        if (cacheFile.Entries.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                cacheFile.Entries.Remove(key);
            }
            else
            {
                entry.LastAccess = DateTimeOffset.UtcNow;
            }
        }
    });

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        Refresh(key);
        return Task.CompletedTask;
    }

    public void Remove(string key) => WithCacheFile(cacheFile =>
    {
        cacheFile.Entries.Remove(key);
    });

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => WithCacheFile(cacheFile =>
    {
        cacheFile.Entries[key] = new CacheEntry()
        {
            Data = value,
            LastAccess = DateTimeOffset.UtcNow,
            AbsoluteExpiration = options.AbsoluteExpiration ??
                (options.AbsoluteExpirationRelativeToNow.HasValue ? DateTimeOffset.UtcNow + options.AbsoluteExpirationRelativeToNow : null),
            SlidingExpiration = options.SlidingExpiration
        };
    });

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }

    private void WithCacheFile(Action<CacheFile> action)
    {
        lock (_gate)
        {
            var cacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TeachingRecordSystem.SupportUi", "cache.json");
            Directory.CreateDirectory(Path.GetDirectoryName(cacheFilePath)!);

            CacheFile cacheFile;

            if (File.Exists(cacheFilePath))
            {
                cacheFile = JsonSerializer.Deserialize<CacheFile>(File.ReadAllText(cacheFilePath))!;
            }
            else
            {
                cacheFile = new CacheFile()
                {
                    Entries = new Dictionary<string, CacheEntry>()
                };
            }

            action(cacheFile);

            File.WriteAllText(cacheFilePath, JsonSerializer.Serialize(cacheFile));
        }
    }

    private class CacheFile
    {
        public required Dictionary<string, CacheEntry> Entries { get; set; }
    }

    private class CacheEntry
    {
        public required byte[] Data { get; set; }
        public DateTimeOffset LastAccess { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public bool IsExpired =>
            AbsoluteExpiration.HasValue && AbsoluteExpiration.Value < DateTimeOffset.UtcNow ||
            SlidingExpiration.HasValue && LastAccess + SlidingExpiration.Value < DateTimeOffset.UtcNow;
    }
}
