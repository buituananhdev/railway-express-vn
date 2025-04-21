using Admin.Application.Dtos;
using Admin.Application.Services;
using Common.Application.Dtos;
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
        await _trainService.CreateAsync(trainDto);
        return Ok();
    }

    [HttpPut("{trainId}")]
    public async Task<IActionResult> UpdateTrain(Guid trainId, [FromBody] AddTrainDto trainDto)
    {
        await _trainService.UpdateAsync(trainId, trainDto);
        return Ok();
    }

    [HttpDelete("{trainId}")]
    public async Task<IActionResult> DeleteTrain(Guid trainId)
    {
        await _trainService.DeleteAsync(trainId);
        return Ok();
    }

    [HttpPost("traincar")]
    public async Task<IActionResult> AddTrainCar([FromBody] AddTrainCarDto trainCarDto)
    {
        await _trainService.CreateTrainCarAsync(trainCarDto);
        return Ok();
    }

    [HttpPut("traincar/{trainCarId}")]
    public async Task<IActionResult> UpdateTrainCar(Guid trainCarId, [FromBody] AddTrainCarDto trainCarDto)
    {
        await _trainService.UpdateTrainCarAsync(trainCarId, trainCarDto);
        return Ok();
    }

    [HttpDelete("traincar/{trainCarId}")]
    public async Task<IActionResult> DeleteTrainCar(Guid trainCarId)
    {
        await _trainService.DeleteTrainCarAsync(trainCarId);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetTrains()
    {
        var trains = await _trainService.GetTrainsAsync();
        return Ok(trains);
    }

    [HttpGet("paging")]
    public async Task<IActionResult> GetTrainsPage([FromQuery] PaginationParams paginationParams)
    {
        var trains = await _trainService.GetListAsync(paginationParams);
        return Ok(trains);
    } 
    [HttpGet("{trainId}/train-cars")]
    public async Task<IActionResult> GetTrainCars(Guid trainId, [FromQuery] Guid trainScheduleId, [FromQuery] DateTime journeyDate)
    {
        var trainCars = await _trainCarService.GetTrainCarsAndPriceAsync(trainId, trainScheduleId, journeyDate);
        return Ok(trainCars);
    }
}
