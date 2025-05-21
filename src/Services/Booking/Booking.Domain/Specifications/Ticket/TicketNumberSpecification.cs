using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications.Ticket;
public class TicketNumberSpecification : Specification<Booking.Domain.Entities.Ticket>
{
    private readonly string _ticketNumber;

    public TicketNumberSpecification(string ticketNumber)
    {
        _ticketNumber = ticketNumber;
    }

    public override Expression<Func<Booking.Domain.Entities.Ticket, bool>> ToExpression()
    {
        return ticket => ticket.TicketNumber == _ticketNumber;
    }
}
