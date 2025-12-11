using System.Text.Json.Serialization;

namespace CoffeePeek.JobVacancies.Models.Responses;

public class HhToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;

    [JsonIgnore]
    public DateTime IssuedAt { get; set; }
}