using Common.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Services.PaymentService;

namespace Payment.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<IPaymentService, PaymentService>();

        services.AddGrpcClient<BookingGrpcService.BookingGrpcServiceClient>(o =>
        {
            o.Address = new Uri(configuration["GRPC:Booking"]);
        });

        return services;
    }
}
