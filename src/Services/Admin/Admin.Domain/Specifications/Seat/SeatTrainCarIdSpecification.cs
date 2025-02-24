using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Admin.Domain.Specifications;
public class SeatTrainCarIdSpecification : Specification<Admin.Domain.Entities.Seat>
{
    private readonly Guid _trainCarId;

    public SeatTrainCarIdSpecification(Guid trainCarId)
    {
        _trainCarId = trainCarId;
    }

    public override Expression<Func<Admin.Domain.Entities.Seat, bool>> ToExpression()
    {
        return seat => seat.TrainCarId == _trainCarId;
    }
}
