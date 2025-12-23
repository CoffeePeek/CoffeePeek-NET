using System.Text.Json.Serialization;

namespace CoffeePeek.Moderation.Infrastructure.External.Yandex;

public class GeocodingResponseData
{
    [JsonPropertyName("GeoObjectCollection")]
    public GeoObjectCollection? GeoObjectCollection { get; set; }
}