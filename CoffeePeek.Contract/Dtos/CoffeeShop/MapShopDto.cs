namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class MapShopDto
{
    public Guid Id { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Title { get; set; } = string.Empty;
}

