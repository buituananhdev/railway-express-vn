using Admin.Application.Dtos;
using Admin.Application.Services.StationService;
using Microsoft.AspNetCore.Mvc;

namespace Admin.API.Controllers
{
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
            await _stationService.AddStationAsync(addStationDto);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetStationsAsync()
        {
            var stations = await _stationService.GetStations();
            return Ok(stations);
        }
    }
}
