using CoffeePeek.Contract.Dtos.Schedule;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.ShopProfile;

public record ShopLocationPatch(Guid CityId, string Address, decimal Latitude, decimal Longitude);

public record ShopContactsPatch(string? PhoneNumber, string? Email, string? SiteLink, string? InstagramLink);

public record ShopCatalogsPatch(
    IReadOnlyList<Guid>? EquipmentIds,
    IReadOnlyList<Guid>? BeanIds,
    IReadOnlyList<Guid>? RoasterIds,
    IReadOnlyList<Guid>? BrewMethodIds);

public record ShopProfilePatch(
    ShopLocationPatch? Location,
    ShopContactsPatch? Contacts,
    IReadOnlyList<ScheduleDto>? Schedules,
    ShopCatalogsPatch? Catalogs);
