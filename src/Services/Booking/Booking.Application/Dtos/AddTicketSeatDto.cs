namespace Booking.Application.Dtos;
public class AddTicketSeatDto
{
    public Guid TicketId { get; set; }
    public Guid SeatId { get; set; }
    public Guid? PassengerInfoId { get; set; }
}
