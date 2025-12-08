using CoffeePeek.Contract.Dtos.Shop;

namespace CoffeePeek.Contract.Response.Internal;

public class GetAllEquipmentResponse(EquipmentDto[] equipments)
{
    public EquipmentDto[] Equipments { get; set; } = equipments;
}