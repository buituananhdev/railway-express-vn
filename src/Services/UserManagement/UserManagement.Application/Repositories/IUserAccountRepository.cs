using Common.Application.Repositories;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Repositories;
public interface IUserAccountRepository : IBaseRepository<UserAccount>
{
}
