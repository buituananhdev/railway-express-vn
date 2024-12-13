using AutoMapper;
using Common.Application.Repositories;
using Passenger.Application.Dtos;
using Passenger.Domain.Specifications;

namespace Passenger.Application.Services
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
                await _passengerRepository.AddAsync(_mapper.Map<Domain.Entities.Passenger>(passengerDto));
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
                throw new Exception(ex.Message);
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
                throw new Exception(ex.Message);
            }
        }
    }
}
