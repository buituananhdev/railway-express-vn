using Common.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Infrastructure.Caching;
public class RedisLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisLockService> _logger;
    private readonly TimeSpan _defaultLockTimeout = TimeSpan.FromSeconds(30);

    public RedisLockService(IConnectionMultiplexer redis, ILogger<RedisLockService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> AcquireLockAsync(string resource, string token, TimeSpan expiryTime)
    {
        if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
        if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

        string lockKey = $"lock:{resource}";
        IDatabase db = _redis.GetDatabase();

        bool acquired = await db.StringSetAsync(lockKey, token, expiryTime, When.NotExists);

        if (acquired)
        {
            _logger.LogDebug("Lock acquired for resource {Resource} with token {Token}", resource, token);
        }
        else
        {
            _logger.LogDebug("Failed to acquire lock for resource {Resource} with token {Token}", resource, token);
        }

        return acquired;
    }

    public async Task<bool> ReleaseLockAsync(string resource, string token)
    {
        if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));
        if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

        string lockKey = $"lock:{resource}";
        IDatabase db = _redis.GetDatabase();

        // Release lock using Lua script to ensure atomic check-and-release
        string script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

        var result = await db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { token });
        bool released = (long)result == 1;

        if (released)
        {
            _logger.LogDebug("Lock released for resource {Resource} with token {Token}", resource, token);
        }
        else
        {
            _logger.LogWarning("Failed to release lock for resource {Resource} with token {Token}", resource, token);
        }

        return released;
    }

    public async Task<T> ExecuteWithLockAsync<T>(string resource, Func<Task<T>> action, TimeSpan? lockTimeout = null)
    {
        string token = Guid.NewGuid().ToString();
        TimeSpan timeout = lockTimeout ?? _defaultLockTimeout;
        bool lockAcquired = false;

        try
        {
            // Try to acquire the lock
            lockAcquired = await AcquireLockAsync(resource, token, timeout);

            if (!lockAcquired)
            {
                throw new InvalidOperationException($"Could not acquire lock for resource '{resource}'");
            }

            // Execute the action if lock was acquired
            return await action();
        }
        finally
        {
            // Release the lock if it was acquired
            if (lockAcquired)
            {
                await ReleaseLockAsync(resource, token);
            }
        }
    }
}
