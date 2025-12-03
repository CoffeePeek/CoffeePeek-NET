using CoffeePeek.Contract.Dtos.Address;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Domain.Enums.Shop;
using Newtonsoft.Json;

namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public class ModerationShopDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NotValidatedAddress { get; set; }
    [JsonIgnore] public int? AddressId { get; set; }
    [JsonIgnore] public int? ShopContactId { get; set; }
    [JsonIgnore] public Guid UserId { get; set; }

    public ModerationStatus ModerationStatus { get; set; }
    public AddressDto Address { get; set; }
    public ShopStatus Status { get; set; }
    public ShopContactDto ShopContact { get; set; }

    public ICollection<string> ShopPhotos { get; set; }
    public ICollection<ScheduleDto> Schedules { get; set; }
}