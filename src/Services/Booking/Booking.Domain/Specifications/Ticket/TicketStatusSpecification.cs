using System.Linq.Expressions;
using Booking.Domain.Enums;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications.Ticket;
public class TicketStatusSpecification : Specification<Booking.Domain.Entities.Ticket>
{
    private readonly TicketStatusEnum _status;

    public TicketStatusSpecification(TicketStatusEnum status)
    {
        _status = status;
    }

    public override Expression<Func<Booking.Domain.Entities.Ticket, bool>> ToExpression()
    {
        return ticket => ticket.Status == _status;
    }
}
