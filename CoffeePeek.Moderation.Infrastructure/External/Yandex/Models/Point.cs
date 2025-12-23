using System.Text.Json.Serialization;

namespace CoffeePeek.Moderation.Infrastructure.External.Yandex;

public class Point
{
    [JsonPropertyName("pos")]
    public string? Pos { get; set; }
}