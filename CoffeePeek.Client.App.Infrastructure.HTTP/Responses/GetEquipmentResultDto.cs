using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetEquipmentResultDto
{
    [JsonPropertyName("equipments")]
    public EquipmentDto[] Equipment { get; init; } = [];
}
