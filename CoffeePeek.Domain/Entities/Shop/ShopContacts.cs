namespace CoffeePeek.Domain.Entities.Shop;

public class ShopContacts : BaseEntity
{
    public int ShopId { get; set; }
    public string PhoneNumber { get; set; }
    public string InstagramLink { get; set; }
    
    public virtual Domain.Entities.Shop.Shop Shop { get; set; }
}