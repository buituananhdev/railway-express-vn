using UserManagement.Application.Dtos;

namespace UserManagement.Application.Services
{
    public interface IPassengerService
    {
        Task AddPassengerAsync(AddPassengerDto passengerDto);
        Task<PassengerDto> GetPassengerByIDAsync(Guid id);
        Task<PassengerDto> GetPassengerByEmailAsync(string email);
    }
}
