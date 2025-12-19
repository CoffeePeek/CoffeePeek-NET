using System.Text.Json.Serialization;

namespace CoffeePeek.ModerationService.Models;

public class YandexGeocodingResponse
{
    [JsonPropertyName("response")]
    public GeocodingResponseData? Response { get; set; }
}