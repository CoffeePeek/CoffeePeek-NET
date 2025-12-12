using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Events.Moderation;

public record CoffeeShopApprovedEvent(
    Guid ModerationShopId,
    string Name,
    string NotValidatedAddress,
    Guid UserId,
    string address,
    Guid? ShopContactId,
    ShopStatus Status,
    ShopContactDto? ShopContact,
    ICollection<string> ShopPhotos,
    ICollection<ScheduleDto> Schedules,
    decimal? Latitude,
    decimal? Longitude
);