using System.Linq.Expressions;
using AutoMapper;
using Common.Application.Dtos;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Application.Repositories;
using Common.Domain.Specifications;
using UserManagement.Application.Dtos;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Specifications;

namespace UserManagement.Application.Services;

internal class PassengerService : IPassengerService
{
    private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserAccountService _userAccountService;
    private readonly IPaginationService _paginationService;
    public PassengerService(IUserManagementUnitOfWork userManagementUnitOfWork, IMapper mapper, IUnitOfWork unitOfWork, IUserAccountService userAccountService, IPaginationService paginationService)
    {
        _userManagementUnitOfWork = userManagementUnitOfWork;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _userAccountService = userAccountService;
        _paginationService = paginationService;
    }
    public async Task AddPassengerAsync(AddPassengerDto passengerDto)
    {
        try
        {
            var account = new UserAccountDto
            {
                Email = passengerDto.Email,
                PasswordHash = passengerDto.Password,
                Role = passengerDto.Role == "Passenger" ? Common.Domain.RoleEnum.Passenger : Common.Domain.RoleEnum.Admin,
                Status = passengerDto.Status ? Common.Domain.StatusEnum.Active : Common.Domain.StatusEnum.Inactive
            };
            var accountDto = await _userAccountService.AddUserAccountAsync(account);

            var passenger = _mapper.Map<Domain.Entities.Passenger>(passengerDto);
            passenger.UserAccountId = accountDto.Id;
            await _userManagementUnitOfWork.PassengerRepository.AddAsync(passenger);
            
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<PassengerDto> GetPassengerByEmailAsync(string email)
    {
        try
        {
            var passenger = await _userManagementUnitOfWork.PassengerRepository.FirstOrDefaultAsync<PassengerDto>(new PassengerEmailSpecification(email!));
            return passenger;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<PassengerDto> GetPassengerByIDAsync(Guid id)
    {
        try
        {
            var passenger = await _userManagementUnitOfWork.PassengerRepository.GetByIdAsync(id);
            return _mapper.Map<PassengerDto>(passenger);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<PaginationResult<PaginatePassengerDto>> GetPassengerListAsync(
        PaginationParams paginationParams,
        Specification<Passenger>? specification = null,
        List<Expression<Func<Passenger, object>>>? includes = null)
    {
        try
        {
            var isActive = paginationParams.IsActive.Value
                ? new PassengerIsActiveSpecification(Common.Domain.StatusEnum.Active)
                : new PassengerIsActiveSpecification(Common.Domain.StatusEnum.Inactive);
            specification = specification == null
                ? isActive
                : specification.And(isActive);
            includes ??= new List<Expression<Func<Passenger, object>>>
            {
                x => x.UserAccount
            };
            var query = _userManagementUnitOfWork
                .PassengerRepository
                .GetQueryable(specification, includes);
            var paginatedResult = await _paginationService
                .CreatePaginatedResultAsync(query, paginationParams);
            return new PaginationResult<PaginatePassengerDto>
            {
                Data = _mapper.Map<List<PaginatePassengerDto>>(paginatedResult.Data),
                CurrentPage = paginatedResult.CurrentPage,
                TotalPages = paginatedResult.TotalPages,
                PageSize = paginatedResult.PageSize,
                TotalCount = paginatedResult.TotalCount
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task DeletePassengerAsync(Guid id)
    {
        try
        {
            var passenger = await _userManagementUnitOfWork.PassengerRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Passenger with id {id} not found");
            var userAccountId = passenger.UserAccountId;
            _userManagementUnitOfWork.PassengerRepository.Delete(passenger);
            await _userAccountService.DeleteUserAccountByIdAsync(userAccountId);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task UpdatePassengerAsync(Guid id, UpdatePassengerDto updatePassengerDto)
    {
        try
        {
            var passenger = await _userManagementUnitOfWork.PassengerRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Passenger with id {id} not found");
            var userAccount = await _userManagementUnitOfWork.UserAccountRepository.GetByIdAsync(passenger.UserAccountId)
                ?? throw new NotFoundException($"User account with id {passenger.UserAccountId} not found");
            userAccount.Email = updatePassengerDto.Email;
            userAccount.PasswordHash = updatePassengerDto.NewPassword == "" ? userAccount.PasswordHash : updatePassengerDto.NewPassword;
            userAccount.Role = updatePassengerDto.Role == "Passenger" ? Common.Domain.RoleEnum.Passenger : Common.Domain.RoleEnum.Admin;
            var updatedUserAccount = new UserAccountDto
            {
                Email = userAccount.Email,
                PasswordHash = userAccount.PasswordHash,
                Role = userAccount.Role,
                Status = userAccount.Status
            };
            await _userAccountService.UpdateUserAccountAsync(userAccount.Id, updatedUserAccount);
            _mapper.Map(updatePassengerDto, passenger);
            _userManagementUnitOfWork.PassengerRepository.Update(passenger);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
