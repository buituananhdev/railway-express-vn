using Booking.Application.Dtos;

namespace Booking.Application.Services;
public interface IPassengerInfoService
{
    Task<PassengerInfoDto> AddPassengerInfoAsync(AddPassengerInfoDto passengerInfoDto);
    Task<List<PassengerInfoDto>> AddPassengerInforsAsync(List<AddPassengerInfoDto> passengerInfoDtos);
}
