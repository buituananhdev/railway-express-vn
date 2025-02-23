using Booking.Application.Dtos;

namespace Booking.Application.Services;
public interface IPassengerInfoService
{
    Task<PassengerInfoDto> AddPassengerInfoAsync(AddPassengerInfoDto passengerInfoDto);
    Task<AddPassengerDetailsDto> AddPassengerDetailsAsync(AddPassengerDetailsDto addPassengerDetailDto);
    Task<List<PassengerInfoDto>> GetPassengerDetailsByTicketIdAsync(Guid ticketId);
}
