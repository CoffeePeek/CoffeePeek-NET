using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class ShortShopDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string[]? ImageUrls { get; set; }
    
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }

    public LocationDto? Location { get; set; }

    public bool IsOpen { get; set; }
    public EquipmentDto[]? Equipments { get; set; }

    public PriceRange PriceRange { get; set; }
}