using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.ShopsService.Controllers.Admin;

namespace CoffeePeek.ShopsService.Controllers.Owner;

public record UpdateOwnerCoffeeShopRequest(
    string Name,
    string? Description,
    string? PhoneNumber,
    string? Email,
    string? SiteLink,
    string? InstagramLink,
    UpdateShopLocationRequest? Location = null,
    IReadOnlyList<ScheduleDto>? Schedules = null,
    UpdateShopCatalogsRequest? Catalogs = null);
