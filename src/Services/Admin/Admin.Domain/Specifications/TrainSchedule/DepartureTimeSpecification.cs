using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Admin.Domain.Specifications;

public class DepartureTimeSpecification : Specification<Admin.Domain.Entities.TrainSchedule>
{
    public DepartureTimeSpecification()
    {
    }

    public override Expression<Func<Admin.Domain.Entities.TrainSchedule, bool>> ToExpression()
    {
        var minimumDepartureTime = DateTime.UtcNow.AddHours(1);
        return schedule => schedule.DepartureTime > minimumDepartureTime;
    }
}
