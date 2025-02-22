using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace Admin.Domain.Specifications;
public class TrainIdSpecification : Specification<Admin.Domain.Entities.TrainCar>
{
    private readonly Guid _trainId;

    public TrainIdSpecification(Guid trainId)
    {
        _trainId = trainId;
    }

    public override Expression<Func<Admin.Domain.Entities.TrainCar, bool>> ToExpression()
    {
        return trainCar => trainCar.TrainId == _trainId;
    }
}
