using Common.Application.Repositories;

namespace Passenger.Application
{
    public interface IPassengerRepository : IBaseRepository<Passenger.Domain.Entities.Passenger>
    {
    }
}
