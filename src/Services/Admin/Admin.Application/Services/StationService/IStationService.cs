using Admin.Application.Dtos;

namespace Admin.Application.Services.StationService
{
    public interface IStationService
    {
        Task AddStationAsync(AddStationDto addStationDto);
        Task<List<StationDto>> GetStations();
    }
}
