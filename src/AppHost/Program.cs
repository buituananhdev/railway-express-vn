var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var rabbitMq = builder.AddRabbitMQ("rabbitmq");
//var elasticSearch = builder.AddElasticSearch("elasticsearch");
//var kibana = builder.AddKibana("kibana")
//                   .WithReference(elasticSearch);

var admin = builder.AddProject<Projects.Admin_API>("admin")
    .WithReference(redis);
var authentication = builder.AddProject<Projects.Auth_API>("authentication")
    .WithReference(redis);
var notification = builder.AddProject<Projects.Notification_API>("notification")
    .WithReference(rabbitMq)
    .WithReference(redis);
var payment = builder.AddProject<Projects.Payment_API>("payment")
    .WithReference(redis);
var usermanagement = builder.AddProject<Projects.UserManagement_API>("usermanagement")
    .WithReference(redis);
var booking = builder.AddProject<Projects.Booking_API>("booking")
    .WithReference(redis);

// API Gateway
var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(admin)
    .WithReference(authentication)
    .WithReference(notification)
    .WithReference(payment)
    .WithReference(usermanagement)
    .WithReference(booking);

// Expose the API Gateway to external access
//builder.AddProject<Projects.MyMicroservices.Web>("webfrontend")
//    .WithReference(apiGateway);

builder.Build().Run();
