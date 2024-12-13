using AutoMapper;
using Common.Infrastructure.Repositories;
using Passenger.Application;

namespace Passenger.Infrastructure
{
    public class PassengerRepository : BaseRepository<Passenger.Domain.Entities.Passenger>, IPassengerRepository
    {
        public PassengerRepository(PassengerContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
