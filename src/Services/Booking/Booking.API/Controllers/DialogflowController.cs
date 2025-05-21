using Booking.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;
[Route("v1/dialogflow")]
[ApiController]
public class DialogflowController : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook([FromBody] DialogflowRequest request)
    {
        var parameters = request.QueryResult.Parameters;

        var missingFields = CheckMissingFields(parameters);
        if (missingFields.Any())
        {
            return Ok(new DialogflowResponse
            {
                FulfillmentText = $"Vui lòng cung cấp thêm: {string.Join(", ", missingFields)}"
            });
        }

        return Ok(new DialogflowResponse
        {
            FulfillmentText = "Vui lòng đợi trong giây lát...",
            Payload = new
            {
                redirect = "http://localhost:5173/ok",
                bookingId = new Guid()
            }
        });
    }

    private List<string> CheckMissingFields(Dictionary<string, object> parameters)
    {
        var requiredFields = new[] { "departure_station", "arrival_station", "date", "quantity", "time" };
        return requiredFields.Where(f => !parameters.ContainsKey(f) || parameters[f] == null).ToList();
    }
}
