using Common.Application.Repositories;
using Common.Infrastructure;
using Common.Infrastructure.Repositories;
using Common.Infrastructure.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Repositories;
using Payment.Infrastructure.Repositories;
using Payment.Infrastructure.VNPayServices;
using VNPAY.NET;

namespace Payment.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<IDataContext>(provider => provider.GetRequiredService<PaymentContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IVnpay, Vnpay>();
        services.AddScoped<IVNPayService, VNPayService>();
        services.AddDbContext<PaymentContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MySQL")
            ?? throw new InvalidOperationException("Connection string 'MySQL' not found.");

            // Specify the MySQL Server Version explicitly
            options.UseMySql(
                connectionString, ServerVersion.AutoDetect(connectionString));
        });

        services.AddScoped<IPaymentUnitOfWork, PaymentUnitOfWork>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
        });

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();

                cfg.Host(settings.Host, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
