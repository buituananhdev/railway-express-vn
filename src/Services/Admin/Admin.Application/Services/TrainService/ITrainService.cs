using Admin.Application.Dtos;

namespace Admin.Application.Services.TrainService
{
    public interface ITrainService
    {
        Task AddTrainAsync(AddTrainDto trainDto);
        Task<List<TrainDto>> GetTrainsAsync();
    }
}
