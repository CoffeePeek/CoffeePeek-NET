using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Domain.Entities.Shop;

public class ShopContacts : BaseEntity
{
    public int ShopId { get; set; }
    [MaxLength(18)]
    public string PhoneNumber { get; set; }
    [MaxLength(50)]
    public string InstagramLink { get; set; }
    
    public virtual Shop Shop { get; set; }
}