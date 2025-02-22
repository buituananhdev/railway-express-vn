using Admin.Application.Dtos;

namespace Admin.Application.Services;
public interface ITrainService
{
    Task AddTrainAsync(AddTrainDto trainDto);
    Task<List<TrainDto>> GetTrainsAsync();

    Task AddTrainCarAsync(AddTrainCarDto trainCarDto);
}
