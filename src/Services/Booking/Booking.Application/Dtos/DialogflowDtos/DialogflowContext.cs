using Newtonsoft.Json;

namespace Booking.Application.Dtos;
public class DialogflowContext
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("lifespanCount")]
    public int LifespanCount { get; set; }

    [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object> Parameters { get; set; }
}
