using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.ModerationService.Models;

public class ShopContacts
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int ShopId { get; set; }
    [MaxLength(18)]
    public string PhoneNumber { get; set; }
    [MaxLength(50)]
    public string InstagramLink { get; set; }
}



