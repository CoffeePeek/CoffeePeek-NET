using System.ComponentModel.DataAnnotations;
using CoffeePeek.Domain.Entities.Users;

namespace CoffeePeek.Domain.Entities.Shop;

public class ShopPhoto : BaseEntity
{
    [MaxLength(70)]
    public string Url { get; set; }
    public int ShopId { get; set; }
    /// <summary>
    /// Creator User ID
    /// </summary>
    public int UserId { get; set; }
    
    public virtual Shop Shop { get; set; }
    public virtual User User { get; set; }
}