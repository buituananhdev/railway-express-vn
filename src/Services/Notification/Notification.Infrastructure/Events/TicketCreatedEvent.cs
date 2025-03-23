using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastructure.Events;
public record TicketCreatedEvent(
        string Email,
        string Subject,
        string Body,
        byte[] PdfContent
    );
