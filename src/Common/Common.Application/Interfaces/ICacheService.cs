namespace Common.Application.Interfaces;
public interface ICacheService
{
    Task SetCacheAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task<T?> GetCacheAsync<T>(string key);
    Task RemoveCacheAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<bool[]> ExistsMultipleAsync(string[] keys);
    Task SetMultipleAsync(List<KeyValuePair<string, string>> entries, TimeSpan expiry);
}
