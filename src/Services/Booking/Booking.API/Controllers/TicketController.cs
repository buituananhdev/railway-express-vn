using Booking.Application.Dtos;
using Booking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;
[Route("api/tickets")]
[ApiController]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost]
    public async Task<IActionResult> AddTicketAsync(AddTicketDto addTicketDto)
    {
        await _ticketService.AddTicketAsync(addTicketDto);
        return Ok();
    }
}
