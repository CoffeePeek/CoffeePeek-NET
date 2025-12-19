using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Moderation.Domain.Entities;

public class ShopContacts
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid ShopId { get; set; }
    [MaxLength(18)]
    public string? PhoneNumber { get; set; }
    [MaxLength(50)]
    public string? InstagramLink { get; set; }
    public string? Email { get; set; }
    public string? SiteLink { get; set; }
}

