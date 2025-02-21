using Common.Application.Repositories;

namespace UserManagement.Application.Repositories;
public interface IUserManagementUnitOfWork : IUnitOfWork
{
    IPassengerRepository PassengerRepository { get; }
    IUserAccountRepository UserAccountRepository { get; }
}
