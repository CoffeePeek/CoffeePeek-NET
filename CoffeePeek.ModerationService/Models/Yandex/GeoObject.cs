using System.Text.Json.Serialization;

namespace CoffeePeek.ModerationService.Models;

public class GeoObject
{
    [JsonPropertyName("Point")]
    public Point? Point { get; set; }
}