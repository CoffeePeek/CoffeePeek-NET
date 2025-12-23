using System.Text.Json.Serialization;

namespace CoffeePeek.Moderation.Infrastructure.External.Yandex;

public class GeoObject
{
    [JsonPropertyName("Point")]
    public Point? Point { get; set; }
}