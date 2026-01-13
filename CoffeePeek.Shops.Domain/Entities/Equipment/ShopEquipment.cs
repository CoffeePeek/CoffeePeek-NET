using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public class ShopEquipment : Entity<Guid>
{
    public Guid ShopId { get; private set; }
    public Guid EquipmentId { get; private set;}
    
    public CoffeeShop CoffeeShop { get; private set; }
    public Equipment Equipment { get; private set; }
    
    // ReSharper disable once UnusedMember.Local
    private ShopEquipment()
    {
        
    }
    public ShopEquipment(Guid equipmentId, Guid id)
    {
        EquipmentId = equipmentId;
        ShopId = id;
    }
}