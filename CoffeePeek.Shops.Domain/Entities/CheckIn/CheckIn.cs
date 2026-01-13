using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Entities;

public class CheckIn
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? ReviewId { get; set; }
    
    public virtual CoffeeShop CoffeeShop { get; set; }
    public virtual ReviewAggregate.Review? Review { get; set; }
}