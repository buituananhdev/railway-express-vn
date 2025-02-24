using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications;
public class TicketSeatIdSpecification : Specification<Booking.Domain.Entities.Ticket>
{
    private readonly Guid _seatId;

    public TicketSeatIdSpecification(Guid seatId)
    {
        _seatId = seatId;
    }

    public override Expression<Func<Booking.Domain.Entities.Ticket, bool>> ToExpression()
    {
        return ticket => ticket.SeatId == _seatId;
    }
}
