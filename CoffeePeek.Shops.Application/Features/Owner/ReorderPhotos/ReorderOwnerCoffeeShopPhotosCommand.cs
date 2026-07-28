namespace CoffeePeek.Shops.Application.Features.Owner.ReorderPhotos;

public record ReorderOwnerCoffeeShopPhotosCommand(
    Guid ShopId,
    Guid OwnerUserId,
    IReadOnlyList<Guid> PhotoIds);
