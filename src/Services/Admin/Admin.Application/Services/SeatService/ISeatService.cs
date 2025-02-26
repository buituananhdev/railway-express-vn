using Admin.Application.Dtos;

namespace Admin.Application.Services;
public interface ISeatService
{
    Task<List<SeatDto>> GetSeatsByTrainCarAndScheduleAsync(Guid trainCarId, Guid trainScheduleId, DateTime journeyDate);
    Task LockSeatsAsync(LockSeatDto lockSeatDto);
    Task<bool> IsSeatLocked(Guid seatId, Guid scheduleId, DateTime journeyDate);
    Task<int> GetAvailableSeatsAsync(Guid trainCarId, Guid scheduleId, DateTime journeyDate);
}
