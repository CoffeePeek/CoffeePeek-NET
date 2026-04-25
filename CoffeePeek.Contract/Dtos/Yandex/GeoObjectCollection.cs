using System.Text.Json.Serialization;

namespace CoffeePeek.Contract.Dtos.Yandex;

public class GeoObjectCollection
{
    [JsonPropertyName("featureMember")]
    public List<FeatureMember>? FeatureMember { get; set; }
}