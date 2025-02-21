using AutoMapper;
using Common.Protos;
using UserManagement.Application.Dtos;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Specifications.Passenger;
using UserManagement.Domain.Specifications.UserAccount;

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

    public async Task AddUserAccountAsync(UserAccountDto userAccountDto)
    {
        try
        {
            var userAccount = _mapper.Map<Domain.Entities.UserAccount>(userAccountDto);
            userAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userAccountDto.PasswordHash);
            await _userManagementUnitOfWork.UserAccountRepository.AddAsync(userAccount);
            await _userManagementUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<UserAccountDto> GetUserAccountByEmailAsync(string email)
    {
        try
        {
            var user = await _userManagementUnitOfWork.UserAccountRepository.FirstOrDefaultAsync<UserAccountDto>(new AccountEmailSpecification(email!));
            return user;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
