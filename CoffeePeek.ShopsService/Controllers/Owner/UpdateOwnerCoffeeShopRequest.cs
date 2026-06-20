namespace CoffeePeek.ShopsService.Controllers.Owner;

public record UpdateOwnerCoffeeShopRequest(
    string Name,
    string? Description,
    string? PhoneNumber,
    string? Email,
    string? SiteLink,
    string? InstagramLink);
