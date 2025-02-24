using Admin.Application.Dtos;

namespace Admin.Application.Services;
public interface ISeatService
{
    Task<List<SeatDto>> GetSeatsByTrainCarAndScheduleAsync(Guid trainCarId, Guid trainScheduleId, DateTime journeyDate);
}
