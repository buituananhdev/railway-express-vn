using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Booking.Domain.Specifications;
public class PassengerInfoTicketIdSpecification : Specification<Booking.Domain.Entities.PassengerInfo>
{
    private readonly Guid _ticketId;

    public PassengerInfoTicketIdSpecification(Guid ticketId)
    {
        _ticketId = ticketId;
    }

    public override Expression<Func<Booking.Domain.Entities.PassengerInfo, bool>> ToExpression()
    {
        return passengerInfo => passengerInfo.TicketId == _ticketId;
    }
}
