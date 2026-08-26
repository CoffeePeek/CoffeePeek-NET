using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Domain.Places;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Application.Features.Public.Stats;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Application.Services;

public class EnrichShopFromImportService(
    IQueryCoffeeShopRepository shopRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<EnrichShopFromImportService> logger) : IEnrichShopFromImportService
{
    public async Task<int> EnrichShopsFromImportAsync(
        IReadOnlyList<ImportShopEnrichmentItem> items,
        CancellationToken ct = default)
    {
        if (items.Count == 0)
            return 0;

        var shops = await shopRepository.ListAllForEnrichmentAsync(ct);
        if (shops.Count == 0)
            return 0;

        var byId = shops.ToDictionary(s => s.Id);
        var enrichedIds = new HashSet<Guid>();

        foreach (var item in items)
        {
            var shop = ResolveShop(item, shops, byId);
            if (shop is null)
                continue;

            if (!shop.TryEnrichFromImport(item.Address, item.Instagram, item.Website, item.Phone))
                continue;

            if (enrichedIds.Add(shop.Id))
            {
                logger.LogInformation(
                    "Enriched published shop {ShopId} ({ShopName}) from import dump",
                    shop.Id,
                    shop.Name);
            }
        }

        if (enrichedIds.Count == 0)
            return 0;

        await unitOfWork.SaveChangesAsync(ct);

        foreach (var shopId in enrichedIds)
            await cacheService.RemoveAsync(CacheKey.Shop.Detail(shopId));

        await cacheService.RemoveByPattern(CacheKey.Shop.SearchPattern(), ct);
        await cacheService.RemoveByPattern(CacheKey.Shop.ListPattern(), ct);
        await cacheService.RemoveByPattern(CacheKey.Shop.DetailPattern(), ct);
        await PublicStatsCacheInvalidator.InvalidateAsync(cacheService, ct);

        return enrichedIds.Count;
    }

    private static CoffeeShop? ResolveShop(
        ImportShopEnrichmentItem item,
        IReadOnlyList<CoffeeShop> shops,
        IReadOnlyDictionary<Guid, CoffeeShop> byId)
    {
        if (item.ShopId is { } shopId && byId.TryGetValue(shopId, out var known))
            return known;

        foreach (var shop in shops)
        {
            if (shop.Location?.Latitude is null || shop.Location.Longitude is null)
                continue;

            if (ShopPlaceMatcher.IsSamePlace(
                    shop.Name,
                    shop.Location.Latitude.Value,
                    shop.Location.Longitude.Value,
                    item.Name,
                    item.Latitude,
                    item.Longitude,
                    shop.Contact?.PhoneNumber,
                    item.Phone,
                    shop.Contact?.InstagramLink,
                    item.Instagram))
                return shop;
        }

        return null;
    }
}
