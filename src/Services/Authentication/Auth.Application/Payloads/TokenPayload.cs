using System.Text.Json.Serialization;

namespace Auth.Application.Payloads
{
    public class TokenPayload
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("access_token")]
        public string? Access { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("refresh_token")]
        public string? Refresh { get; set; }
    }
}
