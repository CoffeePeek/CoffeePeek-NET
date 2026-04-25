using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.Shop;

public class EquipmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public EquipmentCategoryEnum Category { get; set; }
}