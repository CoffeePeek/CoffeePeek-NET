using System.Text.Json.Serialization;

namespace CoffeePeek.Moderation.Infrastructure.External.Yandex;

public class FeatureMember
{
    [JsonPropertyName("GeoObject")]
    public GeoObject? GeoObject { get; set; }
}