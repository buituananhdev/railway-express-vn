using Admin.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.API.Controllers;
[Route("api/seats")]
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
}
