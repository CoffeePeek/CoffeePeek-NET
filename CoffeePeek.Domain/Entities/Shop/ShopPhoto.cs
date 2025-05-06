using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Domain.Entities.Shop;

public class ShopPhoto : BaseEntity
{
    public string Url { get; set; }
    public int ShopId { get; set; }
    /// <summary>
    /// Creator User ID
    /// </summary>
    public int UserId { get; set; }
    
    public virtual Shop Shop { get; set; }
    public virtual User User { get; set; }
}