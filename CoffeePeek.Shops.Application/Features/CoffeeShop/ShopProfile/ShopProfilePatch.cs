using CoffeePeek.Contract.Dtos.Schedule;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.ShopProfile;

/// <summary>Address, coordinates, and city for a published shop.</summary>
public record ShopLocationPatch(Guid CityId, string Address, decimal Latitude, decimal Longitude);

/// <summary>Contact fields. Null string properties clear the stored value.</summary>
public record ShopContactsPatch(string? PhoneNumber, string? Email, string? SiteLink, string? InstagramLink);

/// <summary>
/// Catalog replacements. A null list leaves that catalog unchanged; an empty list clears it.
/// </summary>
public record ShopCatalogsPatch(
    IReadOnlyList<Guid>? EquipmentIds,
    IReadOnlyList<Guid>? BeanIds,
    IReadOnlyList<Guid>? RoasterIds,
    IReadOnlyList<Guid>? BrewMethodIds);

/// <summary>
/// Optional profile sections. A null section is skipped; a present section is applied in full.
/// Empty <see cref="Schedules"/> removes stored hours.
/// </summary>
public record ShopProfilePatch(
    ShopLocationPatch? Location,
    ShopContactsPatch? Contacts,
    IReadOnlyList<ScheduleDto>? Schedules,
    ShopCatalogsPatch? Catalogs);
