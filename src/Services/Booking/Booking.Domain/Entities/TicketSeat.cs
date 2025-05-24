using Common.Domain;

namespace Booking.Domain.Entities;
public class TicketSeat : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; }
    public Guid SeatId { get; set; }
    public Guid? PassengerInfoId { get; set; }
    public PassengerInfo? PassengerInfo { get; set; }
}
