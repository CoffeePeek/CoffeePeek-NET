namespace CoffeePeek.Shops.Domain.Entities;

public class Location : Entity<Guid>
{
    public string Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public Guid ShopId { get; set; }
    public virtual Shop Shop { get; set; }
}