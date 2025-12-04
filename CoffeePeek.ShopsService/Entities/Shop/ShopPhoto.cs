namespace CoffeePeek.ShopsService.Entities;

public class ShopPhoto
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public required string Url { get; set; }

    public virtual Shop Shop { get; set; }
}