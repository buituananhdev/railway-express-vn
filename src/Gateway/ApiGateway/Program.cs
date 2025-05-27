using ApiGateway;
using Common.API.Extentions;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.UseBaseBuilder();

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gateway",
        Version = "v1",
        Description = "API Gateway using YARP"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    options.DocumentFilter<SwaggerDocumentMerger>();
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors(builder =>
{
    builder.WithOrigins(app.Configuration["AllowedHosts"] ?? "localhost")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();

    builder.WithOrigins("http://localhost:5173", "http://localhost:5174", "https://railway-express-vn-booking-u1lb.vercel.app", "https://vetau.site", "https://www.vetau.site/")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
});
app.UseBaseConfig();
app.MapReverseProxy();
app.Run();
