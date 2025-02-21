using AutoMapper;
using Common.Infrastructure;
using Common.Infrastructure.Repositories;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Repositories;
public class UserAccountRepository : BaseRepository<UserAccount>, IUserAccountRepository
{
    public UserAccountRepository(IDataContext context, IMapper mapper) : base(context, mapper)
    {
    }
}
