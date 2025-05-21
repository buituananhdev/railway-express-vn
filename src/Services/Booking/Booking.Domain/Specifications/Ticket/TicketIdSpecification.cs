using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications.Ticket;
public class TicketIdSpecification : Specification<Booking.Domain.Entities.Ticket>
{
    private readonly Guid _id;

    public TicketIdSpecification(Guid id)
    {
        _id = id;
    }

    public override Expression<Func<Booking.Domain.Entities.Ticket, bool>> ToExpression()
    {
        return ticket => ticket.Id == _id;
    }
}
