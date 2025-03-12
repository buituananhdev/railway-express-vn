using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Admin.Domain.Specifications.TrainSchedule;
public class TrainScheduleIdSpecification : Specification<Admin.Domain.Entities.TrainSchedule>
{
    private readonly Guid _Id;

    public TrainScheduleIdSpecification(Guid Id)
    {
        _Id = Id;
    }

    public override Expression<Func<Admin.Domain.Entities.TrainSchedule, bool>> ToExpression()
    {
        return station => station.Id == _Id;
    }
}
