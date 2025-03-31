var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var rabbitMq = builder.AddRabbitMQ("rabbitmq");

builder.AddProject<Projects.Admin_API>("admin-api")
    .WithReference(redis);

builder.AddProject<Projects.Auth_API>("auth-api")
    .WithReference(redis);

builder.AddProject<Projects.Booking_API>("booking-api")
    .WithReference(redis);

builder.AddProject<Projects.Notification_API>("notification-api")
    .WithReference(rabbitMq)
    .WithReference(redis);

builder.AddProject<Projects.Payment_API>("payment-api")
    .WithReference(rabbitMq)
    .WithReference(redis);

builder.AddProject<Projects.UserManagement_API>("usermanagement-api")
    .WithReference(redis);

builder.AddProject<Projects.ApiGateway>("api-gateway");

builder.Build().Run();
