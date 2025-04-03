using System.Linq.Expressions;
using AutoMapper;
using Common.Application.Dtos;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Application.Services;
using Common.Domain.Specifications;
using UserManagement.Application.Dtos;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Specifications;

namespace UserManagement.Application.Services;

internal class PassengerService : BaseService<Passenger, AddPassengerDto, UpdatePassengerDto, PassengerDto>, IPassengerService
{
    private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserAccountService _userAccountService;
    private readonly IPaginationService _paginationService;
    public PassengerService(IUserManagementUnitOfWork userManagementUnitOfWork, IMapper mapper, IUserAccountService userAccountService, IPaginationService paginationService)
        : base(userManagementUnitOfWork.PassengerRepository, userManagementUnitOfWork, mapper, paginationService)
    {
        _userManagementUnitOfWork = userManagementUnitOfWork;
        _mapper = mapper;
        _userAccountService = userAccountService;
        _paginationService = paginationService;
    }
    public async override Task<PassengerDto> CreateAsync(AddPassengerDto passengerDto)
    {
        try
        {
            var account = new UserAccountDto
            {
                Email = passengerDto.Email,
                PasswordHash = passengerDto.Password,
                Role = passengerDto.Role,
                Status = passengerDto.Active
            };
            _userManagementUnitOfWork.BeginTransaction();
            var accountDto = await _userAccountService.AddUserAccountAsync(account);
            var passenger = _mapper.Map<Domain.Entities.Passenger>(passengerDto);
            passenger.UserAccountId = accountDto.Id;
            await _userManagementUnitOfWork.PassengerRepository.AddAsync(passenger);
            await _userManagementUnitOfWork.SaveChangesAsync();
            await _userManagementUnitOfWork.CommitAsync();
            _userManagementUnitOfWork.Dispose();
            return _mapper.Map<PassengerDto>(passenger);
        } 
        catch (Exception ex)
        {
            _userManagementUnitOfWork.Rollback();
            _userManagementUnitOfWork.Dispose();
            throw;
        }
    }

    public async Task<PassengerDto> GetByEmailAsync(string email)
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

    public async override Task<PaginationResult<PassengerDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<Passenger>? specification = null,
        List<Expression<Func<Passenger, object>>>? includes = null)
    {
        try
        {
            var isActive = paginationParams.IsActive.Value
                ? new PassengerStatusSpecification(Common.Domain.StatusEnum.Active)
                : new PassengerStatusSpecification(Common.Domain.StatusEnum.Inactive);
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
            return new PaginationResult<PassengerDto>
            {
                Data = _mapper.Map<List<PassengerDto>>(paginatedResult.Data),
                MetaData = paginatedResult.MetaData
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async override Task<PassengerDto> UpdateAsync(Guid id, UpdatePassengerDto updatePassengerDto)
    {
        try
        {
            var passenger = await _userManagementUnitOfWork.PassengerRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Passenger with id {id} not found");
            var userAccount = await _userManagementUnitOfWork.UserAccountRepository.GetByIdAsync(passenger.UserAccountId)
                ?? throw new NotFoundException($"User account with id {passenger.UserAccountId} not found");
            var updatedUserAccount = new UserAccountDto
            {
                Email = updatePassengerDto.Email,
                Role = updatePassengerDto.Role,
                Status = updatePassengerDto.Active
            };
            _userManagementUnitOfWork.BeginTransaction();
            await _userAccountService.UpdateUserAccountAsync(userAccount.Id, updatedUserAccount);
            _mapper.Map(updatePassengerDto, passenger);
            _userManagementUnitOfWork.PassengerRepository.Update(passenger);
            await _userManagementUnitOfWork.SaveChangesAsync();
            await _userManagementUnitOfWork.CommitAsync();
            _userManagementUnitOfWork.Dispose();
            return _mapper.Map<PassengerDto>(passenger);
        }
        catch (Exception ex)
        {
            _userManagementUnitOfWork.Rollback();
            _userManagementUnitOfWork.Dispose();
            throw;
        }
    }

    public async override Task DeleteAsync(Guid id)
    {
        try
        {
            var passenger = await _userManagementUnitOfWork.PassengerRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Passenger with id {id} not found");
            var userAccountId = passenger.UserAccountId;
            _userManagementUnitOfWork.BeginTransaction();
            _userManagementUnitOfWork.PassengerRepository.Delete(passenger);
            await _userAccountService.DeleteUserAccountByIdAsync(userAccountId);
            await _userManagementUnitOfWork.SaveChangesAsync();
            await _userManagementUnitOfWork.CommitAsync();
            _userManagementUnitOfWork.Dispose();
        }
        catch (Exception ex)
        {
            _userManagementUnitOfWork.Rollback();
            _userManagementUnitOfWork.Dispose();
            throw;
        }
    }
}
