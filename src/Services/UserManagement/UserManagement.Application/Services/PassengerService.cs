using AutoMapper;
using Common.Application.Repositories;
using UserManagement.Application.Dtos;
using UserManagement.Application.Repositories;
using UserManagement.Domain.Specifications;

namespace UserManagement.Application.Services
{
    internal class PassengerService : IPassengerService
    {
        private readonly IPassengerRepository _passengerRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public PassengerService(IPassengerRepository passengerRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _passengerRepository = passengerRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task AddPassengerAsync(AddPassengerDto passengerDto)
        {
            try
            {
                var passenger = _mapper.Map<Domain.Entities.Passenger>(passengerDto);
                passenger.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passengerDto.Password);
                await _passengerRepository.AddAsync(passenger);
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
                var passenger = await _passengerRepository.FirstOrDefaultAsync<PassengerDto>(new EmailSpecification(email!));
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
                var passenger = await _passengerRepository.GetByIdAsync(id);
                return _mapper.Map<PassengerDto>(passenger);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
