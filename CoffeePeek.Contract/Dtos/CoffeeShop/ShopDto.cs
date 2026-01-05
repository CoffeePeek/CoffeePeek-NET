#nullable enable
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class ShopDto 
{
    [JsonIgnore] public Guid Id { get; init; }
    public Guid CityId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public PhotoMetadataDto[] Photos { get; init; }
    
    public decimal Rating { get; init; }
    public int ReviewCount { get; init; }

    public bool IsOpen { get; init; }
    public PriceRange PriceRange { get; init; }
    
    
    public LocationDto? Location { get; init; }
    public BeansDto[]? Beans { get; init; }
    public RoasterDto[]? Roasters { get; init; }
    public EquipmentDto[]? Equipments { get; init; }
    public BrewMethodDto[]? BrewMethods { get; init; }
    public ShopContactDto? ShopContact { get; init; }
    public List<ScheduleDto>? Schedules { get; init; }
}