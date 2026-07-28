using CoffeePeek.Contract.Enums;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Options;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shops.Application.Features.Public.Stats;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.Repositories;
using CoffeePeek.Shops.Domain.Entities;
using Microsoft.Extensions.Options;
using DomainCoffeeShop = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShop;
using DomainCoffeeShopStatus = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShopStatus;
using DomainPriceRange = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.PriceRange;
using ContractPriceRange = CoffeePeek.Contract.Enums.PriceRange;

namespace CoffeePeek.Shops.Application.Features.Admin.Shops;

public record AdminShopPhotoDto(
    Guid Id,
    string FileName,
    string ContentType,
    string StorageKey,
    string FullUrl,
    long SizeBytes,
    int SortIndex);

public record AdminPublishedShopDto(
    Guid Id,
    string Name,
    Guid CityId,
    DomainCoffeeShopStatus Status,
    Guid CreatorId,
    Guid? OwnerUserId,
    Guid? ModerationId,
    DateTime CreatedAtUtc,
    bool IsHidden,
    IReadOnlyList<AdminShopPhotoDto> Photos);

public record GetAdminCoffeeShopsResponse(
    IReadOnlyList<AdminPublishedShopDto> Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize);

public record GetAdminCoffeeShopsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    DomainCoffeeShopStatus? Status = null);

public static class AdminPublishedShopMapper
{
    public static AdminPublishedShopDto Map(DomainCoffeeShop shop, MediaPublicUrlOptions mediaOptions) => new(
        shop.Id,
        shop.Name,
        shop.Location.CityId,
        shop.Status,
        shop.CreatorId,
        shop.OwnerUserId,
        shop.ModerationId,
        shop.CreatedAtUtc,
        shop.Status != DomainCoffeeShopStatus.Active,
        MapPhotos(shop.ShopPhotos, mediaOptions));

    public static IReadOnlyList<AdminShopPhotoDto> MapPhotos(
        IReadOnlyCollection<ShopPhoto> photos,
        MediaPublicUrlOptions mediaOptions) =>
        photos
            .OrderBy(p => p.SortIndex)
            .ThenBy(p => p.CreatedAtUtc)
            .Select(p => new AdminShopPhotoDto(
                p.Id,
                p.FileName,
                p.ContentType,
                p.StorageKey,
                MediaStorageUrlBuilder.BuildPublicUrl(
                    mediaOptions.PublicEndpoint,
                    mediaOptions.ShopBucketName,
                    p.StorageKey) ?? string.Empty,
                p.SizeBytes,
                p.SortIndex))
            .ToList();
}

public static class GetAdminCoffeeShopsHandler
{
    public static async Task<Response<GetAdminCoffeeShopsResponse>> Handle(
        GetAdminCoffeeShopsQuery query,
        IAdminCoffeeShopQueryRepository repository,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await repository.GetPagedAsync(
            page, pageSize, query.Search, query.Status, ct);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var media = mediaOptions.Value;
        var dtos = items.Select(s => AdminPublishedShopMapper.Map(s, media)).ToList();

        return Response<GetAdminCoffeeShopsResponse>.Success(new GetAdminCoffeeShopsResponse(
            dtos, totalCount, totalPages, page, pageSize));
    }
}

public record GetAdminCoffeeShopByIdQuery(Guid ShopId);

public static class GetAdminCoffeeShopByIdHandler
{
    public static async Task<Response<AdminPublishedShopDto>> Handle(
        GetAdminCoffeeShopByIdQuery query,
        ICoffeeShopRepository repository,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(query.ShopId, ct);
        return shop is null
            ? Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.")
            : Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop, mediaOptions.Value));
    }
}

public record UpdateAdminCoffeeShopCommand(
    Guid ShopId,
    string Name,
    string? Description,
    ContractPriceRange PriceRange,
    DomainCoffeeShopStatus? Status);

public static class UpdateAdminCoffeeShopHandler
{
    public static async Task<Response<AdminPublishedShopDto>> Handle(
        UpdateAdminCoffeeShopCommand command,
        ICoffeeShopRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.");

        shop.UpdateDetails(command.Name, command.Description, (DomainPriceRange)command.PriceRange);
        if (command.Status.HasValue)
            shop.SetStatus(command.Status.Value);

        await unitOfWork.SaveChangesAsync(ct);
        await cacheService.RemoveAsync(CacheKey.Shop.Detail(shop.Id));

        return Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop, mediaOptions.Value));
    }
}

public record SetAdminCoffeeShopVisibilityCommand(Guid ShopId, bool Hidden);

public static class SetAdminCoffeeShopVisibilityHandler
{
    public static async Task<Response<AdminPublishedShopDto>> Handle(
        SetAdminCoffeeShopVisibilityCommand command,
        ICoffeeShopRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.");

        shop.SetHidden(command.Hidden);

        await unitOfWork.SaveChangesAsync(ct);
        await cacheService.RemoveAsync(CacheKey.Shop.Detail(shop.Id));
        await PublicStatsCacheInvalidator.InvalidateAsync(cacheService, ct);

        return Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop, mediaOptions.Value));
    }
}

public record AssignCoffeeShopOwnerCommand(Guid ShopId, Guid? OwnerUserId);

public static class AssignCoffeeShopOwnerHandler
{
    public static async Task<Response<AdminPublishedShopDto>> Handle(
        AssignCoffeeShopOwnerCommand command,
        ICoffeeShopRepository repository,
        IUnitOfWork unitOfWork,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.");

        shop.AssignOwner(command.OwnerUserId);

        await unitOfWork.SaveChangesAsync(ct);

        return Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop, mediaOptions.Value));
    }
}

public record ReorderAdminCoffeeShopPhotosCommand(Guid ShopId, IReadOnlyList<Guid> PhotoIds);

public static class ReorderAdminCoffeeShopPhotosHandler
{
    public static async Task<Response<AdminPublishedShopDto>> Handle(
        ReorderAdminCoffeeShopPhotosCommand command,
        ICoffeeShopRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IOptions<MediaPublicUrlOptions> mediaOptions,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.");

        shop.ReorderPhotos(command.PhotoIds);

        await unitOfWork.SaveChangesAsync(ct);
        await cacheService.RemoveAsync(CacheKey.Shop.Detail(shop.Id));

        return Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop, mediaOptions.Value));
    }
}
