namespace CoffeePeek.Shops.Application.Features.Admin.Shops.ReorderPhotos;

public record ReorderAdminCoffeeShopPhotosCommand(Guid ShopId, IReadOnlyList<Guid> PhotoIds);
