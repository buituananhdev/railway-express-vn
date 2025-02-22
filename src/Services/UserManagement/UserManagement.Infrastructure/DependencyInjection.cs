using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Common.Infrastructure;
using UserManagement.Infrastructure.Repositories;
using UserManagement.Application.Repositories;

namespace UserManagement.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<IDataContext>(provider => provider.GetRequiredService<UserManagementContext>());

        services.AddDbContext<UserManagementContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MySQL")
            ?? throw new InvalidOperationException("Connection string 'MySQL' not found.");

            // Specify the MySQL Server Version explicitly
            options.UseMySql(
                connectionString, ServerVersion.AutoDetect(connectionString));
        });

        services.AddScoped<IUserManagementUnitOfWork, UserManagementRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();

        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
        });

        return services;
    }
}
