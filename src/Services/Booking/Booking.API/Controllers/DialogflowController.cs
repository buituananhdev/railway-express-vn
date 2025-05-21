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
            case "booking_ticket":
                var result1 = await _dialogflowService.HandleBookingTicket(parameters);
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
}
