using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications;
public class TicketJourneyDateSpecification : Specification<Booking.Domain.Entities.Ticket>
{
    private readonly DateTime _journeyDate;

    public TicketJourneyDateSpecification(DateTime journeyDate)
    {
        _journeyDate = journeyDate.Date;
    }

    public override Expression<Func<Booking.Domain.Entities.Ticket, bool>> ToExpression()
    {
        return ticket => ticket.JourneyDate.Date == _journeyDate;
    }
}
