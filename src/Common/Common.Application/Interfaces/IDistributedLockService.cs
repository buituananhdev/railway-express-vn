namespace Common.Application.Interfaces;
public interface IDistributedLockService
{
    Task<bool> AcquireLockAsync(string resource, string token, TimeSpan expiryTime);
    Task<bool> ReleaseLockAsync(string resource, string token);
    Task<T> ExecuteWithLockAsync<T>(string resource, Func<Task<T>> action, TimeSpan? lockTimeout = null);
}
