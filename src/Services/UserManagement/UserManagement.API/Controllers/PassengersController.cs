using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Dtos;
using UserManagement.Application.Services;

namespace UserManagement.API.Controllers;

[Route("v1/passengers")]
[ApiController]
public class PassengersController : ControllerBase
{
    private readonly IPassengerService _passengerService;
    public PassengersController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPassengerById(Guid id)
    {
        var passenger = await _passengerService.GetPassengerByIDAsync(id);
        return Ok(passenger);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPassengerByEmail([FromQuery] string email)
    {
        var passenger = await _passengerService.GetPassengerByEmailAsync(email);
        return Ok(passenger);
    }

    [HttpPost]
    public async Task<IActionResult> AddPassenger([FromBody] AddPassengerDto passengerDto)
    {
        await _passengerService.AddPassengerAsync(passengerDto);
        return Ok();
    }
}
