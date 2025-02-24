using UserManagement.Application.Dtos;

namespace UserManagement.Application.Services;
public interface IUserAccountService
{
    Task<UserAccountDto> AddUserAccountAsync(UserAccountDto userAccountDto);
    Task<UserAccountDto> GetUserAccountByEmailAsync(string email);
}
