using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace Admin.Domain.Specifications;
public class StationNameSpecification : Specification<Admin.Domain.Entities.Station>
{
    private readonly string _stationName;

    public StationNameSpecification(string stationName)
    {
        _stationName = stationName;
    }

    public override Expression<Func<Admin.Domain.Entities.Station, bool>> ToExpression()
    {
        return station => station.StationName == _stationName;
    }
}

