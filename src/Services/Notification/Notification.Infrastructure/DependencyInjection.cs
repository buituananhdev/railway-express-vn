using Common.Infrastructure.Settings;
using DinkToPdf;
using DinkToPdf.Contracts;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Consumers;
using Notification.Infrastructure.Services;

namespace Notification.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITemplateService, TemplateService>();

        services.AddRazorPages().AddRazorRuntimeCompilation();

        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        services.AddScoped<IPdfGenerator, PdfGeneratorService>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<EmailConsumer>();
            x.AddConsumer<PaymentSuccessComsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();

                cfg.Host(settings.Host, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ReceiveEndpoint(settings.QueueName, e =>
                {
                    e.ConfigureConsumer<EmailConsumer>(context);
                    e.ConfigureConsumer<PaymentSuccessComsumer>(context);

                    e.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1),
                                                 TimeSpan.FromSeconds(30),
                                                 TimeSpan.FromSeconds(5)));
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
