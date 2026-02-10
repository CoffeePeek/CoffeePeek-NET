using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public record ShortShopDto
{
    public Guid Id { get; init; }
    public Guid CityId { get; init; }
    public required string Name { get; init; }
    public PriceRange PriceRange { get; init; }

    public CoffeeShopStatus Status { get; init; }
    
    public decimal AverageRating { get; init; }
    public int ReviewCount { get; init; }

    public bool IsFavorite { get; init; }
    public bool IsVisited { get; init; }
    public bool IsNew { get; init; }
    public bool IsOpen { get; init; }


    public LocationDto Location { get; init; }
    public ShopContactDto ShopContact { get; init; }
    public BeansDto[] Beans { get; init; } = [];
    public RoasterDto[] Roasters { get; init; } = [];
    public EquipmentDto[] Equipments { get; init; } = [];
    public BrewMethodDto[] BrewMethods { get; init; } = [];
    public ShortPhotoMetadataDto[] Photos { get; init; } = [];
}