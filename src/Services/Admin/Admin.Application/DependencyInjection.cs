using Admin.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<ITrainService, TrainService>();
        services.AddScoped<ITrainScheduleService, TrainScheduleService>();
        services.AddScoped<IStationService, StationService>();

        return services;
    }
}
