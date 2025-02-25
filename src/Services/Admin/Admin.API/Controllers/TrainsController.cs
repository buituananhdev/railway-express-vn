using Admin.Application.Dtos;
using Admin.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.API.Controllers;
[Route("v1/trains")]
[ApiController]
public class TrainsController : ControllerBase
{
    private readonly ITrainService _trainService;
    private readonly ITrainCarService _trainCarService;
    public TrainsController(ITrainService trainService, ITrainCarService trainCarService)
    {
        _trainService = trainService;
        _trainCarService = trainCarService;
    }

    [HttpPost]
    public async Task<IActionResult> AddTrain([FromBody] AddTrainDto trainDto)
    {
        await _trainService.AddTrainAsync(trainDto);
        return Ok();
    }

    [HttpPost("traincar")]
    public async Task<IActionResult> AddTrainCar([FromBody] AddTrainCarDto trainCarDto)
    {
        await _trainService.AddTrainCarAsync(trainCarDto);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetTrains()
    {
        var trains = await _trainService.GetTrainsAsync();
        return Ok(trains);
    }

    [HttpGet("{trainId}/train-cars")]
    public async Task<IActionResult> GetTrainCars(Guid trainId)
    {
        var trainCars = await _trainCarService.GetTrainCarsByTrainIdAsync(trainId);
        return Ok(trainCars);
    }
}
