using Admin.Application.Dtos;

namespace Admin.Application.Services;
public interface ITrainScheduleService
{
    Task AddTrainScheduleAsync(AddTrainScheduleDto trainScheduleDto);
    Task<List<TrainScheduleDto>> GetTrainSchedulesAsync(GetTrainSchedulesDto getTrainSchedulesDto);
}
