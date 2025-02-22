using Admin.Application.Dtos;

namespace Admin.Application.Services;
public interface IStationService
{
    Task AddStationAsync(AddStationDto addStationDto);
    Task<List<StationDto>> GetStations();
}
