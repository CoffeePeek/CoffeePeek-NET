using CoffeePeek.Contract.Enums;
using DomainCoffeeShopStatus = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShopStatus;

namespace CoffeePeek.ShopsService.Controllers.Admin;

public record UpdateAdminCoffeeShopRequest(
    string Name,
    string? Description,
    PriceRange PriceRange,
    DomainCoffeeShopStatus? Status);

public record SetCoffeeShopVisibilityRequest(bool Hidden);

public record AssignCoffeeShopOwnerRequest(Guid? OwnerUserId);
