using Booking.Application.Services;
using Common.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IPassengerInfoService, PassengerInfoService>();
        services.AddScoped<IDialogflowService, DialogflowService>();
        services.AddScoped<ITicketSeatService, TicketSeatService>();

        services.AddGrpcClient<AdminGrpcService.AdminGrpcServiceClient>(o =>
        {
            o.Address = new Uri(configuration["GRPC:Admin"]);
        });

        return services;
    }
}
