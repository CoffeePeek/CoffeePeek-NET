using System.Text.Json.Serialization;

namespace CoffeePeek.Contract.Dtos.Yandex;

public class Point
{
    [JsonPropertyName("pos")]
    public string? Pos { get; set; }
}