using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Dtos;
using Notification.Application.Interfaces;

namespace Notification.API.Controllers;
[Route("v1/tickets")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly IETicketService _eTicketService;

    public NotificationController(IETicketService eTicketService)
    {
        _eTicketService = eTicketService;
    }

    [HttpPost]
    public async Task<IActionResult> SendTicket([FromBody] ETicketDto ticketDto)
    {
        await _eTicketService.SendTicketAsync(ticketDto);
        return Accepted(); // 202 Accepted - request accepted for processing
    }
}
