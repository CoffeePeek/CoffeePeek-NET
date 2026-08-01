using System.Net;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.ShopTagAggregate;

namespace CoffeePeek.Shops.Application.Features.Admin.ShopTags;

public record AdminShopTagDto(
    Guid Id,
    string Slug,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public static class AdminShopTagMapper
{
    public static AdminShopTagDto Map(ShopTag tag) => new(
        tag.Id,
        tag.Slug,
        tag.Name,
        tag.Description,
        tag.SortOrder,
        tag.IsActive,
        tag.CreatedAtUtc,
        tag.UpdatedAtUtc);

    public static ShopTagDto ToPublicDto(ShopTag tag) => new(
        tag.Id,
        tag.Slug,
        tag.Name,
        tag.Description,
        tag.SortOrder);
}

public record GetAllAdminShopTagsQuery;

public static class GetAllAdminShopTagsHandler
{
    public static async Task<Response<AdminShopTagDto[]>> Handle(
        GetAllAdminShopTagsQuery query,
        IQueryShopTagRepository repository,
        CancellationToken ct)
    {
        var tags = await repository.GetAllAsync(ct);
        return Response<AdminShopTagDto[]>.Success(tags.Select(AdminShopTagMapper.Map).ToArray());
    }
}

public record CreateShopTagCommand(
    string Slug,
    string Name,
    string? Description,
    int SortOrder = 0);

public static class CreateShopTagHandler
{
    public static async Task<Response<AdminShopTagDto>> Handle(
        CreateShopTagCommand command,
        IShopTagRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var existing = await repository.GetBySlugAsync(command.Slug, ct);
        if (existing is not null)
            return Response<AdminShopTagDto>.Error(HttpStatusCode.Conflict, "A tag with this slug already exists.");

        var tag = ShopTag.Create(command.Slug, command.Name, command.Description, command.SortOrder);
        repository.Add(tag);
        await unitOfWork.SaveChangesAsync(ct);
        await InvalidateTagCachesAsync(cacheService, ct);

        return Response<AdminShopTagDto>.Success(AdminShopTagMapper.Map(tag));
    }

    internal static async Task InvalidateTagCachesAsync(ICacheService cacheService, CancellationToken ct)
    {
        await cacheService.RemoveByPattern(CacheKey.Shop.TagsCatalogPattern(), ct);
        await cacheService.RemoveByPattern(CacheKey.Shop.SearchPattern(), ct);
    }
}

public record UpdateShopTagCommand(
    [property: JsonIgnore] Guid Id,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive);

public static class UpdateShopTagHandler
{
    public static async Task<Response<AdminShopTagDto>> Handle(
        UpdateShopTagCommand command,
        IShopTagRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var tag = await repository.GetByIdAsync(command.Id, ct);
        if (tag is null)
            return Response<AdminShopTagDto>.Error(HttpStatusCode.NotFound, "Tag not found.");

        tag.Update(command.Name, command.Description, command.SortOrder, command.IsActive);
        await unitOfWork.SaveChangesAsync(ct);
        await CreateShopTagHandler.InvalidateTagCachesAsync(cacheService, ct);

        return Response<AdminShopTagDto>.Success(AdminShopTagMapper.Map(tag));
    }
}

public record DeactivateShopTagCommand([property: JsonIgnore] Guid Id);

public static class DeactivateShopTagHandler
{
    public static async Task<Response<AdminShopTagDto>> Handle(
        DeactivateShopTagCommand command,
        IShopTagRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var tag = await repository.GetByIdAsync(command.Id, ct);
        if (tag is null)
            return Response<AdminShopTagDto>.Error(HttpStatusCode.NotFound, "Tag not found.");

        tag.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
        await CreateShopTagHandler.InvalidateTagCachesAsync(cacheService, ct);

        return Response<AdminShopTagDto>.Success(AdminShopTagMapper.Map(tag));
    }
}
