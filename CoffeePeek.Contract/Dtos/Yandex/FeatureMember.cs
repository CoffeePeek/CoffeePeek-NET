using System.Text.Json.Serialization;

namespace CoffeePeek.Contract.Dtos.Yandex;

public class FeatureMember
{
    [JsonPropertyName("GeoObject")]
    public GeoObject? GeoObject { get; set; }
}