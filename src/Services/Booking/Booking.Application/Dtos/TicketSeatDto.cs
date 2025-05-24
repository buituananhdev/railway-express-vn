using Common.Application.Dtos;

namespace Booking.Application.Dtos;
public class TicketSeatDto : BaseDto
{
    public Guid TicketId { get; set; }
    public Guid SeatId { get; set; }
    public Guid PassengerInfoId { get; set; }
}
