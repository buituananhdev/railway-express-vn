using System.Text.Json.Serialization;

namespace Auth.Application.Payloads;

public class TokenPayload
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("accessToken")]
    public string? Access { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("refreshToken")]
    public string? Refresh { get; set; }
}
