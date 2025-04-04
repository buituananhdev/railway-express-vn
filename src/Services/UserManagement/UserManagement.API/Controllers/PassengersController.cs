using Common.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
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
        var passenger = await _passengerService.GetByIdAsync(id);
        return Ok(passenger);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPassengerByEmail([FromQuery] string email)
    {
        var passenger = await _passengerService.GetByEmailAsync(email);
        return Ok(passenger);
    }

    [HttpPost]
    public async Task<IActionResult> AddPassenger([FromBody] AddPassengerDto passengerDto)
    {
        await _passengerService.CreateAsync(passengerDto);
        return Ok();
    }
    [HttpGet("paging")]
    public async Task<IActionResult> GetPassengersAsync([FromQuery] PaginationParams paginationParams)
    {
        var passengers = await _passengerService.GetListAsync(paginationParams);
        return Ok(passengers);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePassenger(Guid id)
    {
        await _passengerService.DeleteAsync(id);
        return Ok();
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePassenger(Guid id, [FromBody] UpdatePassengerDto updatePassengerDto)
    {
        await _passengerService.UpdateAsync(id, updatePassengerDto);
        return Ok();
    }
}
