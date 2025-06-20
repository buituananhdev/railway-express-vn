using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notification.Infrastructure.Services;
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, byte[] attachment)
    {
        try
        {
            _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);

            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"];

            var from = new EmailAddress(fromEmail, fromName);
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, plainTextContent: null, htmlContent: body);

            if (attachment != null && attachment.Length > 0)
            {
                msg.AddAttachment("vedientu.pdf", Convert.ToBase64String(attachment), "application/pdf");
            }

            var response = await client.SendEmailAsync(msg);

            if ((int)response.StatusCode >= 400)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email. Status: {StatusCode}, Body: {Body}", response.StatusCode, responseBody);
                throw new Exception($"SendGrid failed with status {response.StatusCode}");
            }

            _logger.LogInformation("Email sent successfully via SendGrid.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via SendGrid.");
            throw;
        }
    }
}
