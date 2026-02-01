using System.Text.Json.Serialization;

namespace CoffeePeek.Contract.Dtos.Yandex;

public class YandexGeocodingResponse
{
    [JsonPropertyName("response")]
    public GeocodingResponseData? Response { get; set; }
}