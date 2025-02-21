using AutoMapper;
using Common.Application.Repositories;
using UserManagement.Application.Dtos;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Specifications.Passenger;

namespace UserManagement.Application.Services;

internal class PassengerService : IPassengerService
{
    private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserAccountService _userAccountService;
    public PassengerService(IUserManagementUnitOfWork userManagementUnitOfWork, IMapper mapper, IUnitOfWork unitOfWork, IUserAccountService userAccountService)
    {
        _userManagementUnitOfWork = userManagementUnitOfWork;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _userAccountService = userAccountService;
    }
    public async Task AddPassengerAsync(AddPassengerDto passengerDto)
    {
        try
        {
            var passenger = _mapper.Map<Domain.Entities.Passenger>(passengerDto);
            await _userManagementUnitOfWork.PassengerRepository.AddAsync(passenger);
            var account = new UserAccountDto
            {
                Email = passengerDto.Email,
                PasswordHash = passengerDto.Password,
                Role = Common.Domain.RoleEnum.Passenger,
                Status = Common.Domain.StatusEnum.Active
            };
            await _userAccountService.AddUserAccountAsync(account);
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
}
