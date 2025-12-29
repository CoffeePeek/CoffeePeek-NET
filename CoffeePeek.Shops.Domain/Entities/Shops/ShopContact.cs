namespace CoffeePeek.Shops.Domain.Entities;

public class ShopContact : Entity<Guid>
{
    public Guid ShopId { get; set; }
    public string? InstagramLink { get; set; }
    public string? Email { get; set; }
    public string? SiteLink { get; set; }
    public string? PhoneNumber { get; set; }
    
    public virtual Shop Shop { get; set; }
}