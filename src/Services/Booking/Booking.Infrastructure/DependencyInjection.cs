using Booking.Application.Repositories;
using Booking.Infrastructure.Repositories;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<IDataContext>(provider => provider.GetRequiredService<BookingContext>());

        services.AddDbContext<BookingContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MySQL")
            ?? throw new InvalidOperationException("Connection string 'MySQL' not found.");

            // Specify the MySQL Server Version explicitly
            options.UseMySql(
                connectionString, ServerVersion.AutoDetect(connectionString));
        });

        services.AddScoped<ITicketRepository, TicketRepository>();

        return services;
    }
}
