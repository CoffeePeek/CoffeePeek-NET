namespace CoffeePeek.Contract.Dtos.Menu;

public record AdminShopMenuDto(
    ShopMenuDto Menu,
    IReadOnlyList<UnmatchedMenuItemDto> Unmatched);
