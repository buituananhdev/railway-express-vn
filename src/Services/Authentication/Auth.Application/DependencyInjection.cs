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

        services.AddGrpcClient<User.UserClient>(o =>
        {
            o.Address = new Uri(configuration["GRPC:UserManagement"]);
        });

        return services;
    }
}
