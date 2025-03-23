using Notification.Application.Dtos;

namespace Notification.Application.Interfaces;
public interface IETicketService
{
    Task SendTicketAsync(ETicketDto ticket);
}
