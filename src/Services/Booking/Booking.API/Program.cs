using Common.Infrastructure;
using Booking.Infrastructure;
using Booking.Application;
using Common.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder.UseBaseBuilder();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Common services
builder.Services.AddCommonInfrastructure(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
