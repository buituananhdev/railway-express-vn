using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.Infrastructure.Caching;
public class CacheService
{
    private readonly IDistributedCache _distributedCache;

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetCacheAsync<T>(string key)
    {
        var data = await _distributedCache.GetStringAsync(key);

        if (data == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task RemoveCacheAsync(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }

    public Task SetCacheAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromMinutes(30)
        };

        return _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
    }
}
