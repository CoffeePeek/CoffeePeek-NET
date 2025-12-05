using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.ModerationService.Models;

public class ShopPhoto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string Url { get; set; }
    public int ShopId { get; set; }
    public Guid UserId { get; set; }
}