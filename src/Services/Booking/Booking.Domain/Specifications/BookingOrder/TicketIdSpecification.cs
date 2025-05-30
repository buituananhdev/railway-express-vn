using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications.BookingOrder;
public class TicketIdSpecification : Specification<Booking.Domain.Entities.BookingOrder>
{
    private readonly Guid _ticketId;

    public TicketIdSpecification(Guid ticketId)
    {
        _ticketId = ticketId;
    }

    public override Expression<Func<Booking.Domain.Entities.BookingOrder, bool>> ToExpression()
    {
        return bookingOrder => bookingOrder.Tickets.Any(ticket => ticket.Id == _ticketId);
    }
}
