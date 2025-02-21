using AutoMapper;
using Common.Infrastructure.Repositories;
using Passenger.Application.Repositories;

namespace Passenger.Infrastructure.Repositories
{
    public class PassengerRepository : BaseRepository<Domain.Entities.Passenger>, IPassengerRepository
    {
        public PassengerRepository(UserManagementContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
