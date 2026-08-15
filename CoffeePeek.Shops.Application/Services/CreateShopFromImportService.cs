using CoffeePeek.Contract.Events.Moderation;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Application.Features.Public.Stats;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ShopTagAggregate;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Application.Services;

public class CreateShopFromImportService(
    IQueryCoffeeShopRepository shopRepository,
    IQueryShopTagRepository tagRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<CreateShopFromImportService> logger) : ICreateShopFromImportService
{
    public async Task<Guid> CreateShopFromImportAsync(
        ImportCandidatePublishedItem item,
        CancellationToken cancellationToken = default)
    {
        var existingId = await shopRepository.GetIdByModerationId(item.CandidateId, cancellationToken);
        if (existingId.HasValue)
        {
            logger.LogInformation(
                "Shop {ShopId} already exists for import candidate {CandidateId}",
                existingId.Value,
                item.CandidateId);
            return existingId.Value;
        }

        var shop = new CoffeeShop(
            item.CreatorId,
            item.Name,
            description: null,
            PriceRange.Moderate,
            item.CandidateId);

        shop.SetLocation(item.CityId, item.Address, item.Latitude, item.Longitude);
        shop.SetContact(item.Instagram, email: null, item.Website, item.Phone);
        shop.SetCoffeeFocus((CoffeeFocus)(int)item.CoffeeFocus);

        if (item.TemporarilyClosed)
            shop.SetStatus(CoffeeShopStatus.TemporarilyClosed);

        var slugs = item.TagSlugs.ToList();
        if (item.CoffeeFocus == Contract.Enums.CoffeeFocus.Specialty && !slugs.Contains("specialty"))
            slugs.Add("specialty");

        var tags = await tagRepository.GetActiveBySlugsAsync(slugs, cancellationToken);
        if (tags.Length > 0)
            shop.SetTags(tags.Select(t => t.Id).ToArray(), item.CreatorId);

        shopRepository.Add(shop);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveByPattern(CacheKey.Shop.SearchPattern(), cancellationToken);
        await cacheService.RemoveByPattern(CacheKey.Shop.ListPattern(), cancellationToken);
        await cacheService.RemoveByPattern(CacheKey.Shop.DetailPattern(), cancellationToken);
        await cacheService.RemoveAsync(CacheKey.Shop.Detail(shop.Id));
        await PublicStatsCacheInvalidator.InvalidateAsync(cacheService, cancellationToken);

        logger.LogInformation(
            "Shop {ShopId} created from import candidate {CandidateId}",
            shop.Id,
            item.CandidateId);

        return shop.Id;
    }
}
