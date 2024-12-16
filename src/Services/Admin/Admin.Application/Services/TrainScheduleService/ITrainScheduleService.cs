using Admin.Application.Dtos;

namespace Admin.Application.Services.TrainScheduleService
{
    public interface ITrainScheduleService
    {
        Task AddTrainScheduleAsync(AddTrainScheduleDto trainScheduleDto);
        Task<List<TrainScheduleDto>> GetTrainSchedulesAsync();
    }
}
