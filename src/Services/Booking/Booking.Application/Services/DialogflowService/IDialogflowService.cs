using Booking.Application.Dtos;

namespace Booking.Application.Services;
public interface IDialogflowService
{
    Task<DialogflowResponse> HandleBookingTicket(Dictionary<string, object> parameters, string session);
    Task<DialogflowResponse> HandleSearchTicket(Dictionary<string, object> parameters);
    Task<(string FulfillmentText, object Payload)> DetectIntentWithPayloadAsync(string sessionId, string text);
    Task<DialogflowResponse> HandleCheckTicketAvailability(Dictionary<string, object> parameters, string session);
}
