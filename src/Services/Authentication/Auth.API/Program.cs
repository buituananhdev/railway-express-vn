using Auth.Application;
using Common.API.Extentions;

var builder = WebApplication.CreateBuilder(args);
builder.UseBaseBuilder();

builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();
app.UseBaseConfig();
app.Run();
