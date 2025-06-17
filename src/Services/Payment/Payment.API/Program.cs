using Common.API.Extentions;
using Payment.Application;
using Payment.Infrastructure;
using Payment.Infrastructure.GrpcServices;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.UseBaseBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcService<PaymentService>();

app.UseBaseConfig();

app.Run();
