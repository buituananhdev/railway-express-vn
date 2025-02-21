using AutoMapper;
using Common.Infrastructure.Repositories;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Repositories
{
    public class PassengerRepository : BaseRepository<Passenger>, IPassengerRepository
    {
        public PassengerRepository(UserManagementContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
