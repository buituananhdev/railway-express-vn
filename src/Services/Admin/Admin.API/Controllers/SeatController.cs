using Admin.Application.Dtos;
using Admin.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.API.Controllers;
[Route("v1/seats")]
[ApiController]
public class SeatController : ControllerBase
{
    private readonly ISeatService _seatService;

    public SeatController(ISeatService seatService)
    {
        _seatService = seatService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSeats([FromQuery] Guid trainCarId, Guid trainScheduleId, DateTime journeyDate)
    {
        var seats = await _seatService.GetSeatsByTrainCarAndScheduleAsync(trainCarId, trainScheduleId, journeyDate);
        return Ok(seats);
    }

    [HttpPost("lock")]
    public async Task<IActionResult> LockSeats(LockSeatDto lockSeatDto)
    {
        await _seatService.LockSeatsAsync(lockSeatDto);
        return Ok();
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableSeats([FromQuery] Guid seatId)
    {
        var availableSeats = await _seatService.GetSeatWithTrainInformationAsync(seatId);
        return Ok(availableSeats);
    }
}
