using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Admin.Domain.Specifications.Seat;
public class SeatIdSpecification : Specification<Admin.Domain.Entities.Seat>
{
    private readonly Guid _seatId;

    public SeatIdSpecification(Guid seatId)
    {
        _seatId = seatId;
    }

    public override Expression<Func<Admin.Domain.Entities.Seat, bool>> ToExpression()
    {
        return seat => seat.Id == _seatId;
    }
}

