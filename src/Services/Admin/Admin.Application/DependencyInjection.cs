using Admin.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Protos;

namespace Admin.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<ITrainService, TrainService>();
        services.AddScoped<ITrainScheduleService, TrainScheduleService>();
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<ISeatService, SeatService>();
        services.AddScoped<ITrainCarService, TrainCarService>();

        services.AddGrpcClient<BookingGrpcService.BookingGrpcServiceClient>(o =>
        {
            o.Address = new Uri("http://localhost:7002");
        });

        return services;
    }
}
