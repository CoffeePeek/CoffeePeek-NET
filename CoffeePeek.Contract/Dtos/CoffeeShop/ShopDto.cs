using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class ShopDto 
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string[]? ImageUrls { get; set; }
    
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }

    public LocationDto? Location { get; set; }

    public bool IsOpen { get; set; }

    public BeansDto[]? Beans { get; set; }
    public RoasterDto[]? Roasters { get; set; }
    public EquipmentDto[]? Equipments { get; set; }

    public PriceRange PriceRange { get; set; }
    
    public string? Description { get; set; }
    public ShopContactDto? ShopContact { get; set; }
    public List<ScheduleDto>? Schedules { get; set; }
}