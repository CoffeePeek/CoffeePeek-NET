namespace CoffeePeek.ShopsService.Entities;

public class Location
{
    public Guid Id { get; set; }
    public string Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public Guid ShopId { get; set; }
    public virtual Shop Shop { get; set; }
}