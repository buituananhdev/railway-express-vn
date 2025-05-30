using Common.Domain;

namespace Booking.Domain.Entities;
public class BookingOrder : BaseEntity
{
    public ICollection<Ticket> Tickets { get; set; }
}
