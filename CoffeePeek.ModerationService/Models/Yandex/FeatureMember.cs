using System.Text.Json.Serialization;

namespace CoffeePeek.ModerationService.Models;

public class FeatureMember
{
    [JsonPropertyName("GeoObject")]
    public GeoObject? GeoObject { get; set; }
}