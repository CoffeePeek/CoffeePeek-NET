using System.Text.Json.Serialization;

namespace CoffeePeek.Domain.Enums.Shop;

public enum ReviewStatus
{
    [JsonPropertyName("Pending")]
    Pending,

    [JsonPropertyName("Approved")]
    Approved,

    [JsonPropertyName("Rejected")]
    Rejected
}