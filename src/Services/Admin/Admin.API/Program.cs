using Admin.Application;
using Admin.Infrastructure;
using Common.API.Extentions;
using Admin.Infrastructure.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.UseBaseBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcService<AdminService>();

app.UseBaseConfig();

app.Run();
