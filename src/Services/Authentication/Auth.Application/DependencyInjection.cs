using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Auth.Application.Services;
using Common.Protos;

namespace Auth.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGreeterGrpcService, GreeterGrpcService>();
        services.AddGrpcClient<Greeter.GreeterClient>(o =>
        {
            o.Address = new Uri("http://localhost:7004");
        });

        services.AddGrpcClient<User.UserClient>(o =>
        {
            o.Address = new Uri("http://localhost:7004");
        });

        return services;
    }
}
