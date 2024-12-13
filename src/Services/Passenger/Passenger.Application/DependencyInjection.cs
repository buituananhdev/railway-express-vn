using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passenger.Application.Services;

namespace Passenger.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddScoped<IPassengerService, PassengerService>();

            return services;
        }
    }
}
