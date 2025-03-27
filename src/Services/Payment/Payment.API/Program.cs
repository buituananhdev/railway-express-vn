using Common.API.Extentions;
using Payment.Application;
using Payment.Infrastructure;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);
builder.UseBaseBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();
app.UseBaseConfig();

app.Run();
