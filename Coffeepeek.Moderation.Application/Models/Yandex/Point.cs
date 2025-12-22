using System.Text.Json.Serialization;

namespace CoffeePeek.ModerationService.Models;

public class Point
{
    [JsonPropertyName("pos")]
    public string? Pos { get; set; }
}