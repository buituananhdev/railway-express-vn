using Booking.Application.Dtos;

namespace Booking.Application.Services;
public interface IDialogflowService
{
    Task<DialogflowResponse> HandleBookingTicket(Dictionary<string, object> parameters);
    Task<DialogflowResponse> HandleSearchTicket(Dictionary<string, object> parameters);
}
