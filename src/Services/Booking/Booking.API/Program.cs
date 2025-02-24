using Booking.Application;
using Booking.Infrastructure;
using Booking.Infrastructure.GrpcServices;
using Common.API.Extentions;
using Common.Protos;

var builder = WebApplication.CreateBuilder(args);

builder.UseBaseBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<BookingService>();

app.UseBaseConfig();
app.Run();
