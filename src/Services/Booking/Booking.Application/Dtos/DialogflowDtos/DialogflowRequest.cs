using Newtonsoft.Json;

namespace Booking.Application.Dtos;
public class DialogflowRequest
{
    [JsonProperty("queryResult")]
    public QueryResult QueryResult { get; set; }
}

public class QueryResult
{
    public Dictionary<string, object> Parameters { get; set; }
    public Intent Intent { get; set; }
}

public class Intent
{
    public string DisplayName { get; set; }
}
