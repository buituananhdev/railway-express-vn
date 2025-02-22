using Booking.Application;
using Booking.Infrastructure;
using Common.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder.UseBaseBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();
app.UseBaseConfig();
app.Run();
