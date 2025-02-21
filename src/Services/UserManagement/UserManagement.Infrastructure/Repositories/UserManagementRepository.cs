using Common.Infrastructure;
using Common.Infrastructure.Repositories;
using UserManagement.Application.Repositories;

namespace UserManagement.Infrastructure.Repositories;
public class UserManagementRepository : UnitOfWork, IUserManagementUnitOfWork
{
    public IPassengerRepository PassengerRepository { get; private set; }
    public IUserAccountRepository UserAccountRepository { get; private set; }

    public UserManagementRepository(
        IPassengerRepository passengerRepository,
        IUserAccountRepository userAccountRepository,
        IDataContext context) : base(context)
    {
        PassengerRepository = passengerRepository;
        UserAccountRepository = userAccountRepository;
    }
}
