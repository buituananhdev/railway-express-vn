using Common.Domain;

namespace Notification.Domain.Entities;
public class NotificationRecord : BaseEntity
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public string Recipient { get; set; }
    public string Sender { get; set; }
}
