using Booking.Application.Dtos;
using Booking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;
[Route("api/tickets")]
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
        var ticket = await _ticketService.AddTicketAsync(addTicketDto);
        return Ok(ticket);
    }

    [HttpPost("passenger-details")]
    public async Task<IActionResult> AddPassengerDetailsAsync(List<AddPassengerInfoDto> passengerInfoDtos)
    {
        var passengerDetails = await _passengerInfoService.AddPassengerInforsAsync(passengerInfoDtos);
        return Ok(passengerDetails);
    }
}
