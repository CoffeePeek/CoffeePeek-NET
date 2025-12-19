using System.Text.Json.Serialization;

namespace CoffeePeek.ModerationService.Models;

public class GeocodingResponseData
{
    [JsonPropertyName("GeoObjectCollection")]
    public GeoObjectCollection? GeoObjectCollection { get; set; }
}