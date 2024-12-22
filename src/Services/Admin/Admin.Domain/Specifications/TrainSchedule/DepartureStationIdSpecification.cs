using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace Admin.Domain.Specifications
{
    public class DepartureStationIdSpecification : Specification<Admin.Domain.Entities.TrainSchedule>
    {
        private readonly Guid _departureStationId;

        public DepartureStationIdSpecification(Guid departureStationId)
        {
            _departureStationId = departureStationId;
        }

        public override Expression<Func<Admin.Domain.Entities.TrainSchedule, bool>> ToExpression()
        {
            return station => station.DepartureStationId == _departureStationId;
        }
    }
}
