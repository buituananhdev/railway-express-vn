using Common.Application.Interfaces;
using Common.Application.Repositories;
using Common.Infrastructure.Caching;
using Common.Infrastructure.Logging;
using Common.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.Elasticsearch;
using Serilog;

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
        services.AddScoped<ICacheService, CacheService>();

        //Log.Logger = new LoggerConfiguration()
        //        .Enrich.FromLogContext()
        //        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticSearch:Uri"]))
        //        {
        //            AutoRegisterTemplate = true,
        //            IndexFormat = $"{configuration["ElasticSearch:DefaultIndex"]}-{DateTime.UtcNow:yyyy.MM.dd}",
        //        })
        //        .CreateLogger();

        return services;
    }

    public static IHostBuilder UseSerilogLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog();
        return hostBuilder;
    }
}
