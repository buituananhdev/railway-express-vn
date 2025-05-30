using Booking.Domain.Entities;

namespace Booking.Application.Dtos;
public class AddBookingOrderDto
{
    public ICollection<Ticket> Tickets { get; set; }
}
