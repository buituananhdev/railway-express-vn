using Admin.Application.Dtos;
using Admin.Application.Services;
using Common.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Admin.API.Controllers;
[Route("v1/stations")]
[ApiController]
public class StationsController : ControllerBase
{
    private readonly IStationService _stationService;

    public StationsController(IStationService stationService)
    {
        _stationService = stationService;
    }

    [HttpPost]
    public async Task<IActionResult> AddStationAsync(AddStationDto addStationDto)
    {
        await _stationService.CreateAsync(addStationDto);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStationByIdAsync(Guid id)
    {
        var station = await _stationService.GetByIdAsync(id);
        return Ok(station);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteStationAsync(Guid id)
    {
        await _stationService.DeleteAsync(id);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStationAsync(Guid id, AddStationDto updateStationDto)
    {
        await _stationService.UpdateAsync(id, updateStationDto);
        return Ok();
    }

    [HttpGet("paging")]
    public async Task<IActionResult> GetStationsAsync([FromQuery] PaginationParams paginationParams)
    {
        var stations = await _stationService.GetListAsync(paginationParams);
        return Ok(stations);
    }

    [HttpGet]
    public async Task<IActionResult> GetStationsAsync()
    {
        var stations = await _stationService.GetStations();
        return Ok(stations);
    }
}
