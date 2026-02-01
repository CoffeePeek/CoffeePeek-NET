using System.Text.Json.Serialization;

namespace CoffeePeek.Contract.Dtos.Yandex;

public class GeoObject
{
    [JsonPropertyName("Point")]
    public Point? Point { get; set; }
}