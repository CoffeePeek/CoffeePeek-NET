using CoffeePeek.Contract.Dtos.Address;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Domain.Enums.Shop;

namespace CoffeePeek.Contract.Events.Moderation;

public record CoffeeShopApprovedEvent(
    int ModerationShopId,
    string Name,
    string NotValidatedAddress,
    Guid UserId,
    int? AddressId,
    int? ShopContactId,
    ShopStatus Status,
    AddressDto? Address,
    ShopContactDto? ShopContact,
    ICollection<string> ShopPhotos,
    ICollection<ScheduleDto> Schedules
);

