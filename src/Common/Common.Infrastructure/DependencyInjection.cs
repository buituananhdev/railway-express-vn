using Common.Application.Interfaces;
using Common.Infrastructure.Caching;
using Common.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Common.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(redisOptions =>
        {
            var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
            redisOptions.Configuration = connectionString;
        });

        services.AddSingleton<ILoggerService, SerilogLogger>();
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IDistributedLockService, RedisLockService>();

        return services;
    }
}
