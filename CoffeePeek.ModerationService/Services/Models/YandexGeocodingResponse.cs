using System.Text.Json.Serialization;

namespace CoffeePeek.ModerationService.Services.Models;

public class YandexGeocodingResponse
{
    [JsonPropertyName("response")]
    public Response? Response { get; set; }
}

public class Response
{
    [JsonPropertyName("GeoObjectCollection")]
    public GeoObjectCollection? GeoObjectCollection { get; set; }
}

public class GeoObjectCollection
{
    [JsonPropertyName("featureMember")]
    public List<FeatureMember>? FeatureMember { get; set; }
}

public class FeatureMember
{
    [JsonPropertyName("GeoObject")]
    public GeoObject? GeoObject { get; set; }
}

public class GeoObject
{
    [JsonPropertyName("Point")]
    public Point? Point { get; set; }
}

public class Point
{
    [JsonPropertyName("pos")]
    public string? Pos { get; set; }
}

