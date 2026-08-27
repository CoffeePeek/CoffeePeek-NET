using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using DomainCoffeeShopStatus = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShopStatus;

namespace CoffeePeek.ShopsService.Controllers.Admin;

public record UpdateShopLocationRequest(
    Guid CityId,
    string Address,
    decimal Latitude,
    decimal Longitude);

public record UpdateShopContactsRequest(
    string? PhoneNumber,
    string? Email,
    string? SiteLink,
    string? InstagramLink);

public record UpdateShopCatalogsRequest(
    IReadOnlyList<Guid>? EquipmentIds,
    IReadOnlyList<Guid>? BeanIds,
    IReadOnlyList<Guid>? RoasterIds,
    IReadOnlyList<Guid>? BrewMethodIds);

public record UpdateAdminCoffeeShopRequest(
    string Name,
    string? Description,
    PriceRange PriceRange,
    DomainCoffeeShopStatus? Status,
    UpdateShopLocationRequest? Location = null,
    UpdateShopContactsRequest? Contacts = null,
    IReadOnlyList<ScheduleDto>? Schedules = null,
    UpdateShopCatalogsRequest? Catalogs = null);

public record SetCoffeeShopVisibilityRequest(bool Hidden);

public record AssignCoffeeShopOwnerRequest(Guid? OwnerUserId);

/// <summary>Full permutation of gallery photo IDs in desired display order (index 0 = cover).</summary>
public record ReorderCoffeeShopPhotosRequest(IReadOnlyList<Guid> PhotoIds);

public record AddCoffeeShopPhotosRequest(IReadOnlyList<UploadedPhotoDto> Photos);

public record RemoveCoffeeShopPhotosRequest(IReadOnlyList<Guid> PhotoIds);

public record SetCoffeeShopTagsRequest(Guid[] TagIds);

public record SetCoffeeShopFocusRequest(CoffeeShopType Type);
