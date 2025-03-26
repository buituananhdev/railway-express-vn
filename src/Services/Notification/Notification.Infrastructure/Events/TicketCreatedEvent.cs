namespace Notification.Infrastructure.Events;
public record TicketCreatedEvent(
        string Email,
        string Subject,
        string Body,
        byte[] PdfContent
    );
