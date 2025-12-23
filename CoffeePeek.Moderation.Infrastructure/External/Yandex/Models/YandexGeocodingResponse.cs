using System.Text.Json.Serialization;

namespace CoffeePeek.Moderation.Infrastructure.External.Yandex;

public class YandexGeocodingResponse
{
    [JsonPropertyName("response")]
    public GeocodingResponseData? Response { get; set; }
}