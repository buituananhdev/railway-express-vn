using MassTransit;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Events;

namespace Notification.Infrastructure.Consumers;
public class EmailConsumer : IConsumer<TicketCreatedEvent>
{
    private readonly IEmailService _emailService;

    public EmailConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var message = context.Message;
        await _emailService.SendEmailAsync(
            message.Email,
            message.Subject,
            message.Body,
            message.PdfContent
        );
    }
}
