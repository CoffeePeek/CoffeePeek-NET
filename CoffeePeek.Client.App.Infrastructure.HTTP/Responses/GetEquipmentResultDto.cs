using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetEquipmentResultDto
{
    public EquipmentDto[] Equipments { get; set; } = [];
}
