using Admin.Application;
using Admin.Infrastructure;
using Common.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.UseBaseBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseBaseConfig();

app.Run();
