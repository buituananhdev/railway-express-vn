using Booking.Application.Dtos;

namespace Booking.Application.Services;
public interface ITicketService
{
    Task<TicketDto> AddTicketAsync(AddTicketDto addTicketDto);
}
