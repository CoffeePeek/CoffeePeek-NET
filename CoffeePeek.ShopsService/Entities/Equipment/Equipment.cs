namespace CoffeePeek.ShopsService.Entities;

public class Equipment
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public virtual ICollection<ShopEquipment> ShopEquipments { get; set; } = new HashSet<ShopEquipment>();
}