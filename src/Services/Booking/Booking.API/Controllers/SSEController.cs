using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SSEController : ControllerBase
{
    private readonly ISSEPublisher _publisher;

    public SSEController(ISSEPublisher publisher) => _publisher = publisher;

    [HttpGet("{sessionId}")]
    public async Task Get(string sessionId)
    {
        await _publisher.RegisterClientAsync(sessionId, HttpContext);
    }
}
