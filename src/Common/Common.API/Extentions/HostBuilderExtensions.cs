using System.Text;
using Common.API.Helper;
using Common.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Common.Infrastructure;
using Common.Application;
using Azure.Identity;
using Serilog.Sinks.Elasticsearch;
using Serilog;

namespace Common.API.Extentions;
public static class HostBuilderExtensions
{
    public static void UseBaseBuilder(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            var keyVaultUrl = new Uri($"https://railway-vault.vault.azure.net/");
            builder.Configuration.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());
        }
        else
        {
            builder.Configuration.AddJsonFile(Path.Combine(PathHelper.GetRootDirectory(), "Common", "Common.API", "appsettings.common.json"), optional: false, reloadOnChange: true);
        }

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        if(builder.Environment.IsProduction())
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["ElasticSearch:Uri"]))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = $"{builder.Configuration["ElasticSearch:DefaultIndex"]}-{DateTime.UtcNow:yyyy.MM.dd}",
                })
                .CreateLogger();
        } else
        {
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.Console()
                 .CreateLogger();
        }
        builder.Host.UseSerilogLogging();

        builder.Services.AddCommonApplication(builder.Configuration);
        builder.Services.AddCommonInfrastructure(builder.Configuration);

        var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtSettings:Secret").Value!);
        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            x.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                }
            };
        });

    }

    public static WebApplication UseBaseConfig(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        return app;
    }

    public static IHostBuilder UseSerilogLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog();
        return hostBuilder;
    }
}
