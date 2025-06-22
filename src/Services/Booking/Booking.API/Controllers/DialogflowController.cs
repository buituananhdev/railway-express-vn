using Booking.Application.Dtos;
using Booking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;
[Route("v1/dialogflow")]
[ApiController]
public class DialogflowController : ControllerBase
{
    private readonly IDialogflowService _dialogflowService;

    public DialogflowController(IDialogflowService dialogflowService)
    {
        _dialogflowService = dialogflowService;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook([FromBody] DialogflowRequest request)
    {
        var intentName = request.QueryResult.Intent.DisplayName;
        var parameters = request.QueryResult.Parameters;

        switch (intentName)
        {
            case "BookTicket.CheckRouteAvailability":
                var availabilityResult = await _dialogflowService.HandleCheckTicketAvailability(parameters, request.Session);
                return Ok(availabilityResult);

            case "BookTicket.PassengerInfo":
                parameters = request.QueryResult.OutputContexts?
                    .FirstOrDefault(c => c.Name.EndsWith("available_route_confirmed")).Parameters;
                var result1 = await _dialogflowService.HandleBookingTicket(parameters, request.Session);
                return Ok(result1);

            case "search_ticket":
                var result2 = await _dialogflowService.HandleSearchTicket(parameters);
                return Ok(result2);

            default:
                return Ok(new DialogflowResponse
                {
                    FulfillmentText = "Xin lỗi, tôi chưa hỗ trợ yêu cầu này."
                });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UserMessage request)
    {
        var sessionReply = await _dialogflowService.DetectIntentWithPayloadAsync(request.SessionId, request.Text);

        return Ok(new
        {
            reply = sessionReply.FulfillmentText,
            payload = sessionReply.Payload
        });
    }

    public class UserMessage
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; } = string.Empty;
    }
}
