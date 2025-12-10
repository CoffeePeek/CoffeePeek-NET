using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Contract.Responses.Internal;

public class GetAllEquipmentResponse(EquipmentDto[] equipments)
{
    public IReadOnlyList<EquipmentDto> Equipments { get; set; } = equipments;
}