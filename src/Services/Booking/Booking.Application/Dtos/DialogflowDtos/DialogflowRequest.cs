using Newtonsoft.Json;

namespace Booking.Application.Dtos
{
    public class DialogflowRequest
    {
        [JsonProperty("queryResult")]
        public QueryResult QueryResult { get; set; }

        [JsonProperty("session")]
        public string Session { get; set; }
    }

    public class QueryResult
    {
        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters { get; set; }

        [JsonProperty("intent")]
        public Intent Intent { get; set; }

        [JsonProperty("outputContexts")]
        public List<DialogflowContext> OutputContexts { get; set; }
    }

    public class Intent
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}
