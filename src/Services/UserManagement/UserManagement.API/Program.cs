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

# region gRPC configrations
app.MapGrpcService<GreeterService>();
app.MapGrpcService<UserService>();
# endregion

app.UseBaseConfig();
app.Run();
