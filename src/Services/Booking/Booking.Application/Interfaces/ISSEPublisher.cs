using Microsoft.AspNetCore.Http;

namespace Booking.Application.Interfaces;
public interface ISSEPublisher
{
    Task RegisterClientAsync(string sessionId, HttpContext context);

    Task SendAsync(string sessionId, string eventName, object data);
}
