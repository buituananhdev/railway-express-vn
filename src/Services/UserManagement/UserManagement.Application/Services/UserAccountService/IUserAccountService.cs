using UserManagement.Application.Dtos;

namespace UserManagement.Application.Services;
public interface IUserAccountService
{
    Task AddUserAccountAsync(AddUserAccountDto userAccountDto);
}
