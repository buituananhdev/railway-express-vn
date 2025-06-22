using Newtonsoft.Json;

namespace Booking.Application.Dtos;
public class DialogflowResponse
{
    [JsonProperty("fulfillmentText")]
    public string FulfillmentText { get; set; }

    [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
    public object Payload { get; set; }

    [JsonProperty("outputContexts", NullValueHandling = NullValueHandling.Ignore)]
    public List<DialogflowContext> OutputContexts { get; set; }
}
