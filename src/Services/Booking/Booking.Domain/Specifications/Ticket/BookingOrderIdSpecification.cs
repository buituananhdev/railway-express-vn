using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications.Ticket;
public class BookingOrderIdSpecification : Specification<Booking.Domain.Entities.Ticket>
{
    private readonly Guid _bookingOrderId;

    public BookingOrderIdSpecification(Guid bookingOrderId)
    {
        _bookingOrderId = bookingOrderId;
    }

    public override Expression<Func<Booking.Domain.Entities.Ticket, bool>> ToExpression()
    {
        return ticket => ticket.BookingOrderId == _bookingOrderId;
    }
}
