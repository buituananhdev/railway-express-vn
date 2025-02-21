using AutoMapper;
using UserManagement.Application.Dtos;
using UserManagement.Application.Repositories;

namespace UserManagement.Application.Services;
public class UserAccountService : IUserAccountService
{
    private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
    private readonly IMapper _mapper;

    public UserAccountService(IUserManagementUnitOfWork userManagementUnitOfWork, IMapper mapper)
    {
        _userManagementUnitOfWork = userManagementUnitOfWork;
        _mapper = mapper;
    }

    public async Task AddUserAccountAsync(AddUserAccountDto userAccountDto)
    {
        try
        {
            var userAccount = _mapper.Map<Domain.Entities.UserAccount>(userAccountDto);
            userAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userAccountDto.Password);
            await _userManagementUnitOfWork.UserAccountRepository.AddAsync(userAccount);
            await _userManagementUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
