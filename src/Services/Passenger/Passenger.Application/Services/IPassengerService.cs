using Passenger.Application.Dtos;

namespace Passenger.Application.Services
{
    public interface IPassengerService
    {
        Task AddPassengerAsync(AddPassengerDto passengerDto);
        Task<PassengerDto> GetPassengerByIDAsync(Guid Id);
    }
}
