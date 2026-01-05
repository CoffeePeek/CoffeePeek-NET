using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Shops.Domain.Entities;

public class Location : Entity<Guid>
{
    [MaxLength(200)]
    public string Address { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public Guid ShopId { get; set; }
    public virtual Shop Shop { get; set; }
}