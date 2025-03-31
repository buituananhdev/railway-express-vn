using AutoMapper;
using Common.Application.Exceptions;
using Common.Protos;
using UserManagement.Application.Dtos;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Specifications;

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

    public async Task<UserAccountDto> AddUserAccountAsync(UserAccountDto userAccountDto)
    {
        try
        {
            var userAccount = _mapper.Map<Domain.Entities.UserAccount>(userAccountDto);
            userAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userAccountDto.PasswordHash);
            await _userManagementUnitOfWork.UserAccountRepository.AddAsync(userAccount);
            await _userManagementUnitOfWork.SaveChangesAsync();

            return _mapper.Map<UserAccountDto>(userAccount);
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

    public async Task DeleteUserAccountByIdAsync(Guid id)
    {
        try
        {
            var userAccount = await _userManagementUnitOfWork.UserAccountRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"User account with id {id} not found");
            _userManagementUnitOfWork.UserAccountRepository.Delete(userAccount);
            await _userManagementUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task UpdateUserAccountAsync(Guid id, UserAccountDto updateUserAccountDto)
    {
        try
        {
            var userAccount = await _userManagementUnitOfWork.UserAccountRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"User account with id {id} not found");

            userAccount.Email = updateUserAccountDto.Email;
            if (!string.IsNullOrEmpty(updateUserAccountDto.PasswordHash))
            {
                userAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserAccountDto.PasswordHash);
            }
            userAccount.Role = updateUserAccountDto.Role;
            userAccount.Status = updateUserAccountDto.Status;
            await _userManagementUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
