using Booking.Application.Dtos;
using Booking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;
[Route("v1/tickets")]
[ApiController]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IPassengerInfoService _passengerInfoService;
    public TicketController(ITicketService ticketService, IPassengerInfoService passengerInfoService)
    {
        _ticketService = ticketService;
        _passengerInfoService = passengerInfoService;
    }

    [HttpPost]
    public async Task<IActionResult> AddTicketAsync(AddTicketDto addTicketDto)
    {
        var ticket = await _ticketService.CreateAsync(addTicketDto);
        return Ok(ticket);
    }

    [HttpPost("passenger-details")]
    public async Task<IActionResult> AddPassengerDetailsAsync(AddPassengerDetailsDto addPassengerDetailDto)
    {
        var passengerDetails = await _passengerInfoService.AddPassengerDetailsAsync(addPassengerDetailDto);
        return Ok(passengerDetails);
    }

    [HttpGet("{ticketId}/passenger-details")]
    public async Task<IActionResult> GetPassengerDetailsByTicketIdAsync(Guid ticketId)
    {
        var passengerDetails = await _passengerInfoService.GetPassengerDetailsByTicketIdAsync(ticketId);
        return Ok(passengerDetails);
    }

    [HttpGet("{ticketId}/cancel")]
    public async Task<IActionResult> CancelTicketAsync(Guid ticketId)
    {
        await _ticketService.CancelTicketAsync(ticketId);
        return NoContent();
    }

    [HttpGet("{ticketId}")]
    public async Task<IActionResult> GetTicketByIdAsync(Guid ticketId)
    {
        var ticket = await _ticketService.GetByIdAsync(ticketId);
        return Ok(ticket);
    }

    [HttpGet("ticket-number/{ticketNumber}")]
    public async Task<IActionResult> GetTicketByTicketNumberAsync([FromRoute] string ticketNumber)
    {
        var ticket = await _ticketService.GetTicketByTicketNumberAsync(ticketNumber);
        return Ok(ticket);
    }
}
