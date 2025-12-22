using System.Text.Json.Serialization;

namespace CoffeePeek.ModerationService.Models;

public class GeoObjectCollection
{
    [JsonPropertyName("featureMember")]
    public List<FeatureMember>? FeatureMember { get; set; }
}