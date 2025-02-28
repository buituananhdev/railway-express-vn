using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications;
public class TicketSeatIdsSpecification : Specification<Booking.Domain.Entities.Ticket>
{
    private readonly ICollection<Guid> _seatIds;

    public TicketSeatIdsSpecification(ICollection<Guid> seatIds)
    {
        _seatIds = seatIds;
    }

    public override Expression<Func<Booking.Domain.Entities.Ticket, bool>> ToExpression()
    {
        return ticket => ticket.SeatIds.Equals(_seatIds);
    }
}
