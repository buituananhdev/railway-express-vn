using Common.API.Extentions;
using UserManagement.Application;
using UserManagement.Infrastructure;
using UserManagement.Infrastructure.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.UseBaseBuilder();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcService<UserService>();

app.UseBaseConfig();
app.Run();
