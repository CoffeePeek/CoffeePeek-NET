using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Admin.ShopTags;
using CoffeePeek.Shops.Domain.Aggregates.ShopTagAggregate;

namespace CoffeePeek.Shops.Application.Features.Catalogs.GetShopTags;

public record GetShopTagsCommand;

public record GetShopTagsResponse(ShopTagDto[] Tags);

public static class GetShopTagsHandler
{
    public static async Task<Response<GetShopTagsResponse>> Handle(
        GetShopTagsCommand command,
        IQueryShopTagRepository repository,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var cacheKey = CacheKey.Shop.TagsCatalog();

        var tags = await cacheService.GetAsync(cacheKey, async token =>
        {
            var active = await repository.GetActiveAsync(token);
            return active.Select(AdminShopTagMapper.ToPublicDto).ToArray();
        }, cancellationToken: ct);

        return tags is null
            ? Response<GetShopTagsResponse>.Error("Failed to retrieve shop tags.")
            : Response<GetShopTagsResponse>.Success(new GetShopTagsResponse(tags));
    }
}
