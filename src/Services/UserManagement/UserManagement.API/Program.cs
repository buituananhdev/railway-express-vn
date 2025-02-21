using Common.Infrastructure;
using UserManagement.Infrastructure.GrpcServices;
using UserManagement.Application;
using UserManagement.Infrastructure;
using Common.API.Extentions;

var builder = WebApplication.CreateBuilder(args);
// Common services
builder.UseBaseBuilder();

builder.Services.AddCommonInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);


var app = builder.Build();

# region gRPC configrations
app.MapGrpcService<GreeterService>();
app.MapGrpcService<UserService>();
# endregion

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
