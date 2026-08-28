using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.Repositories;

public interface ICoffeeShopRepository
{
    Task<CoffeeShop?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CoffeeShop?> GetByIdForOwnerAsync(Guid id, Guid ownerUserId, CancellationToken ct = default);
    Task<CoffeeShop?> GetByIdWithCatalogsAsync(Guid id, CancellationToken ct = default);
    Task<CoffeeShop?> GetByIdWithCatalogsForOwnerAsync(Guid id, Guid ownerUserId, CancellationToken ct = default);

    /// <summary>
    /// Inserts gallery photos without tracking the shop aggregate (owned schedules +
    /// sibling collections). Loading that graph made SaveChanges issue a 0-row UPDATE.
    /// </summary>
    Task<bool> TryAttachGalleryPhotosAsync(
        Guid shopId,
        Guid? ownerUserId,
        IReadOnlyList<ShopPhoto> photos,
        CancellationToken ct = default);
}

public interface IAdminCoffeeShopQueryRepository
{
    Task<(IReadOnlyList<CoffeeShop> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CoffeeShopStatus? status,
        CancellationToken ct = default,
        bool? importedFromFile = null);

    Task<IReadOnlyList<CoffeeShop>> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken ct = default);
}
