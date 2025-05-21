using Newtonsoft.Json;

namespace Booking.Application.Dtos;
public class DialogflowResponse
{
    [JsonProperty("fulfillmentText")]
    public string FulfillmentText { get; set; }

    [JsonProperty("payload")]
    public object Payload { get; set; }
}
