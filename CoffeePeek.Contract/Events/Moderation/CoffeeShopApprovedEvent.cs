using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Events.Moderation;

public record CoffeeShopApprovedEvent(
    int ModerationShopId,
    string Name,
    string NotValidatedAddress,
    Guid UserId,
    string address,
    int? ShopContactId,
    ShopStatus Status,
    ShopContactDto? ShopContact,
    ICollection<string> ShopPhotos,
    ICollection<ScheduleDto> Schedules
);