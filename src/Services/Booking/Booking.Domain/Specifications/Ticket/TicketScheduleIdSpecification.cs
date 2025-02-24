using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications;
public class TicketScheduleIdSpecification : Specification<Booking.Domain.Entities.Ticket>
{
    private readonly Guid _scheduleId;

    public TicketScheduleIdSpecification(Guid scheduleId)
    {
        _scheduleId = scheduleId;
    }

    public override Expression<Func<Booking.Domain.Entities.Ticket, bool>> ToExpression()
    {
        return ticket => ticket.TrainScheduleId == _scheduleId;
    }
}
