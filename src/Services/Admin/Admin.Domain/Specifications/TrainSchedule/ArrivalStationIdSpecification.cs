using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace Admin.Domain.Specifications;
public class ArrivalStationIdSpecification : Specification<Admin.Domain.Entities.TrainSchedule>
{
    private readonly Guid _arrivalStationId;

    public ArrivalStationIdSpecification(Guid arrivalStationId)
    {
        _arrivalStationId = arrivalStationId;
    }

    public override Expression<Func<Admin.Domain.Entities.TrainSchedule, bool>> ToExpression()
    {
        return station => station.ArrivalStationId == _arrivalStationId;
    }
}
