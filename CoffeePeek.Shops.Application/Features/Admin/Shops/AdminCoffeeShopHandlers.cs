using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.Repositories;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using DomainCoffeeShop = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShop;
using DomainCoffeeShopStatus = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.CoffeeShopStatus;
using DomainPriceRange = CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate.PriceRange;
using ContractPriceRange = CoffeePeek.Contract.Enums.PriceRange;

namespace CoffeePeek.Shops.Application.Features.Admin.Shops;

public record AdminPublishedShopDto(
    Guid Id,
    string Name,
    Guid CityId,
    DomainCoffeeShopStatus Status,
    Guid CreatorId,
    Guid? OwnerUserId,
    Guid? ModerationId,
    DateTime CreatedAtUtc,
    bool IsHidden);

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
    public static AdminPublishedShopDto Map(DomainCoffeeShop shop) => new(
        shop.Id,
        shop.Name,
        shop.Location.CityId,
        shop.Status,
        shop.CreatorId,
        shop.OwnerUserId,
        shop.ModerationId,
        shop.CreatedAtUtc,
        shop.Status != DomainCoffeeShopStatus.Active);
}

public static class GetAdminCoffeeShopsHandler
{
    public static async Task<Response<GetAdminCoffeeShopsResponse>> Handle(
        GetAdminCoffeeShopsQuery query,
        IAdminCoffeeShopQueryRepository repository,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await repository.GetPagedAsync(
            page, pageSize, query.Search, query.Status, ct);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var dtos = items.Select(AdminPublishedShopMapper.Map).ToList();

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
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(query.ShopId, ct);
        return shop is null
            ? Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.")
            : Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop));
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

        return Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop));
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
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.");

        shop.SetHidden(command.Hidden);

        await unitOfWork.SaveChangesAsync(ct);
        await cacheService.RemoveAsync(CacheKey.Shop.Detail(shop.Id));

        return Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop));
    }
}

public record AssignCoffeeShopOwnerCommand(Guid ShopId, Guid? OwnerUserId);

public static class AssignCoffeeShopOwnerHandler
{
    public static async Task<Response<AdminPublishedShopDto>> Handle(
        AssignCoffeeShopOwnerCommand command,
        ICoffeeShopRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return Response<AdminPublishedShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Shop not found.");

        shop.AssignOwner(command.OwnerUserId);

        await unitOfWork.SaveChangesAsync(ct);

        return Response<AdminPublishedShopDto>.Success(AdminPublishedShopMapper.Map(shop));
    }
}
