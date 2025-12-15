using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.ModerationService.Entities;

public class ShopPhoto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public required string Url { get; set; }
    public Guid ShopId { get; set; }
    public Guid UserId { get; set; }
}
