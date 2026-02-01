using System.Text.Json.Serialization;

namespace CoffeePeek.Contract.Dtos.Yandex;

public class GeocodingResponseData
{
    [JsonPropertyName("GeoObjectCollection")]
    public GeoObjectCollection? GeoObjectCollection { get; set; }
}