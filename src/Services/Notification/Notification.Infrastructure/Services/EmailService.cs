using Microsoft.Extensions.Configuration;
using Notification.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Notification.Infrastructure.Services;
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body, byte[] attachment)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration.GetSection("SMTPConfigs:Displayname").Value, _configuration.GetSection("SMTPConfigs:Email").Value));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };

            builder.Attachments.Add("ticket.pdf", attachment, ContentType.Parse("application/pdf"));

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_configuration.GetSection("SMTPConfigs:Host").Value, _configuration.GetValue<int>("SMTPConfigs:Port"), false);
            await client.AuthenticateAsync(_configuration.GetSection("SMTPConfigs:Displayname").Value, _configuration.GetSection("SMTPConfigs:Password").Value);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
