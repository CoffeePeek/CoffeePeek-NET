namespace CoffeePeek.ShopsService.Entities;

public class ShopContact
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string? InstagramLink { get; set; }
    public string? Email { get; set; }
    public string? SiteLink { get; set; }
    public string? PhoneNumber { get; set; }
    
    public virtual Shop Shop { get; set; }
}