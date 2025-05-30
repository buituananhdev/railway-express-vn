using Booking.Domain.Entities;
using Common.Application.Dtos;

namespace Booking.Application.Dtos;
public class BookingOrderDto : BaseDto
{
    public ICollection<Ticket> Tickets { get; set; }
}
