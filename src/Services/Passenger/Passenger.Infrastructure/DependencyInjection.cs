using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Passenger.Application;
using Common.Infrastructure;

namespace Passenger.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDataContext>(provider => provider.GetRequiredService<PassengerContext>());

            services.AddDbContext<PassengerContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("MySQL")   
                ?? throw new InvalidOperationException("Connection string 'MySQL' not found.");

                // Specify the MySQL Server Version explicitly
                options.UseMySql(
                    connectionString, ServerVersion.AutoDetect(connectionString));
            });

            services.AddScoped<IPassengerRepository, PassengerRepository>();

            return services;
        }
    }
}
