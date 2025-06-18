using Booking.Application.Dtos;

namespace Booking.Application.Services;
public interface IDialogflowService
{
    Task<DialogflowResponse> HandleBookingTicket(Dictionary<string, object> parameters, string session);
    Task<DialogflowResponse> HandleSearchTicket(Dictionary<string, object> parameters);
    Task<string> DetectIntentAsync(string sessionId, string text);
}
