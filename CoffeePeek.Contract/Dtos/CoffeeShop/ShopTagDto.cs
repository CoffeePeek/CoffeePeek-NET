namespace CoffeePeek.Contract.Dtos.CoffeeShop;

public record ShopTagDto(
    Guid Id,
    string Slug,
    string Name,
    string? Description,
    int SortOrder);
