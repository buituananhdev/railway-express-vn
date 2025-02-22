using Common.Application.Interfaces;
using Common.Application.Pagination;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddCommonApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPaginationService, PaginationService>();
        return services;
    }
}
