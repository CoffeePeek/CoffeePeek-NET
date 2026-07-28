using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Options;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Features.Admin.Shops;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.Repositories;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shops.Application.Features.Owner;

public record GetOwnerCoffeeShopsQuery(Guid OwnerUserId);

public record GetOwnerCoffeeShopsResponse(IReadOnlyList<AdminPublishedShopDto> Items);

public static class GetOwnerCoffeeShopsHandler
{
    public static async Task<Response<GetOwnerCoffeeShopsResponse>> Handle(
        GetOwnerCoffeeShopsQuery query,
        IAdminCoffeeShopQueryRepository repository,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var items = await repository.GetByOwnerUserIdAsync(query.OwnerUserId, ct);
        var media = mediaOptions.Value;
        var dtos = items.Select(s => AdminPublishedShopMapper.Map(s, media)).ToList();
        return Response<GetOwnerCoffeeShopsResponse>.Success(new GetOwnerCoffeeShopsResponse(dtos));
    }
}

public record GetOwnerCoffeeShopByIdQuery(Guid ShopId, Guid OwnerUserId);

public static class GetOwnerCoffeeShopByIdHandler
{
    public static async Task<Response<AdminPublishedShopDto>> Handle(
        GetOwnerCoffeeShopByIdQuery query,
        ICoffeeShopRepository repository,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdForOwnerAsync(query.ShopId, query.OwnerUserId, ct);
        return shop is null
            ? Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.")
            : Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop, mediaOptions.Value));
    }
}

public record UpdateOwnerCoffeeShopCommand(
    Guid ShopId,
    Guid OwnerUserId,
    string Name,
    string? Description,
    string? PhoneNumber,
    string? Email,
    string? SiteLink,
    string? InstagramLink);

public static class UpdateOwnerCoffeeShopHandler
{
    public static async Task<Response<AdminPublishedShopDto>> Handle(
        UpdateOwnerCoffeeShopCommand command,
        ICoffeeShopRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdForOwnerAsync(command.ShopId, command.OwnerUserId, ct);
        if (shop is null)
            return Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.");

        shop.UpdateDetails(command.Name, command.Description, shop.PriceRange);
        shop.SetContact(command.InstagramLink, command.Email, command.SiteLink, command.PhoneNumber);

        await unitOfWork.SaveChangesAsync(ct);
        await cacheService.RemoveAsync(CacheKey.Shop.Detail(shop.Id));

        return Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop, mediaOptions.Value));
    }
}
