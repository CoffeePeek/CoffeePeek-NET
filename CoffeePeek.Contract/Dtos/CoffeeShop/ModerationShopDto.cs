using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using Newtonsoft.Json;

namespace CoffeePeek.Contract.Dtos.CoffeeShop;

#nullable enable

public class ModerationShopDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string NotValidatedAddress { get; set; } = null!;
    public string? Description { get; set; }
    public PriceRange? PriceRange { get; set; }
    public Guid? CityId { get; set; }
    public Guid UserId { get; set; }
    public ModerationStatus ModerationStatus { get; set; }
    public ShopStatus Status { get; set; }
    public ShopContactDto? ShopContact { get; set; }
    public List<ScheduleDto>? Schedules { get; set; }
    public List<Guid>? EquipmentIds { get; set; }
    public List<Guid>? CoffeeBeanIds { get; set; }
    public List<Guid>? RoasterIds { get; set; }
    public List<Guid>? BrewMethodIds { get; set; }
    public List<string>? ShopPhotos { get; set; }
}