using System.Text.Json.Serialization;

namespace CoffeePeek.JobVacancies.Models;

public class HhWorkFormat
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}