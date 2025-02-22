using Common.Application.Repositories;
using Common.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddStackExchangeRedisCache(redisOptions =>
        {
            var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
            redisOptions.Configuration = connectionString;
        });
        return services;
    }
}
