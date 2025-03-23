namespace Common.Infrastructure.Settings;
public class RabbitMQSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "ticket-queue";

    // Optional: Retry configuration
    public int RetryCount { get; set; } = 3;
    public int RetryInterval { get; set; } = 30; // seconds

    // Optional: SSL configuration
    public bool UseSsl { get; set; } = false;
    public string SslServerName { get; set; } = string.Empty;
}
