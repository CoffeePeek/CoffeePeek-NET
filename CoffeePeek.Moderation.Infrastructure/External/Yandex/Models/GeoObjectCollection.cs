using System.Text.Json.Serialization;

namespace CoffeePeek.Moderation.Infrastructure.External.Yandex;

public class GeoObjectCollection
{
    [JsonPropertyName("featureMember")]
    public List<FeatureMember>? FeatureMember { get; set; }
}