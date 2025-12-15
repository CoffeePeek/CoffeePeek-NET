using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.ShopsService.Entities.CheckIn;

public class CheckIn
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? ReviewId { get; set; }
    
    public virtual Shop Shop { get; set; }
    public virtual Review? Review { get; set; }
}