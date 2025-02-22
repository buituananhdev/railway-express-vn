using Admin.Application.Repositories;
using Admin.Infrastructure.Repositories;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AdminContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MySQL")   
            ?? throw new InvalidOperationException("Connection string 'MySQL' not found.");

            // Specify the MySQL Server Version explicitly
            options.UseMySql(
                connectionString, ServerVersion.AutoDetect(connectionString));
        });

        services.AddScoped<ITrainRepository, TrainRepository>();
        services.AddScoped<ITrainCarRepository, TrainCarRepository>();
        services.AddScoped<ITrainStatusRepository, TrainStatusRepository>();
        services.AddScoped<ITrainScheduleRepository, TrainScheduleRepository>();
        services.AddScoped<IStationRepository, StationRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();

        services.AddScoped<IAdminUnitOfWork, AdminUnitOfWork>();

        services.AddScoped<IDataContext>(provider => provider.GetRequiredService<AdminContext>());

        return services;
    }
}
