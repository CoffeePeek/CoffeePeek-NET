namespace CoffeePeek.ShopsService.Entities;

public class ShopEquipment
{
    public Guid Id { get; set; }
    
    public Guid ShopId { get; set; }
    public Guid EquipmentId { get; set;}
    
    public virtual Shop Shop { get; set; }
    public virtual Equipment Equipment { get; set; }
}