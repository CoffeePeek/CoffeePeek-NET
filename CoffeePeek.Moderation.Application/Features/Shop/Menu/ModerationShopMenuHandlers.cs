using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.Menu;
using CoffeePeek.Contract.Events.Menu;
using CoffeePeek.Moderation.Application.Features.Menu;
using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;
using DomainPriceRange = CoffeePeek.Moderation.Domain.Aggregates.Enums.PriceRange;

namespace CoffeePeek.Moderation.Application.Features.Shop.Menu;

public record AttachModerationShopMenuPhotosCommand(
    Guid ShopId,
    IReadOnlyList<UploadedPhotoDto> Photos,
    Guid UserId);

public static class AttachModerationShopMenuPhotosHandler
{
    public static async Task<(Response<ModerationShopDto>, ParseMenuRequestedEvent?)> Handle(
        AttachModerationShopMenuPhotosCommand command,
        IModerationShopRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return (Response<ModerationShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Coffee shop not found"), null);

        shop.AttachMenuPhotos(
            command.Photos.Select(p => (p.FileName, p.ContentType, p.StorageKey, p.Size)).ToArray(),
            DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(ct);

        return (
            Response<ModerationShopDto>.Success(mapper.Map<ModerationShopDto>(shop)),
            MenuParseEvents.FromDraft(
                MenuParseSourceKind.ModerationShop,
                shop.Id,
                shop.ShopId == Guid.Empty ? null : shop.ShopId,
                shop.Menu,
                command.UserId));
    }
}

public record ParseModerationShopMenuCommand(Guid ShopId, Guid UserId);

public static class ParseModerationShopMenuHandler
{
    public static async Task<(Response<ModerationShopDto>, ParseMenuRequestedEvent?)> Handle(
        ParseModerationShopMenuCommand command,
        IModerationShopRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return (Response<ModerationShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Coffee shop not found"), null);

        try
        {
            shop.RequestMenuParse();
        }
        catch (DomainException ex)
        {
            return (Response<ModerationShopDto>.Error(System.Net.HttpStatusCode.BadRequest, ex.Message), null);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return (
            Response<ModerationShopDto>.Success(mapper.Map<ModerationShopDto>(shop)),
            MenuParseEvents.FromDraft(
                MenuParseSourceKind.ModerationShop,
                shop.Id,
                shop.ShopId == Guid.Empty ? null : shop.ShopId,
                shop.Menu,
                command.UserId));
    }
}

public record UpdateModerationShopMenuCommand(
    Guid ShopId,
    IReadOnlyList<UpdateShopMenuItemRequest> Items,
    bool ApplySuggestedPriceRange,
    Guid UserId);

public static class UpdateModerationShopMenuHandler
{
    public static async Task<(Response<ModerationShopDto>, ApplyShopMenuSnapshotEvent?)> Handle(
        UpdateModerationShopMenuCommand command,
        IModerationShopRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CancellationToken ct)
    {
        var shop = await repository.GetByIdAsync(command.ShopId, ct);
        if (shop is null)
            return (Response<ModerationShopDto>.Error(System.Net.HttpStatusCode.NotFound, "Coffee shop not found"), null);

        shop.ReplaceMenuItems(MenuDraftMapper.ToDraftItems(command.Items), DateTime.UtcNow);

        if (command.ApplySuggestedPriceRange && shop.Menu?.SuggestedPriceRange is { } range)
            shop.ApplySuggestedPriceRange((DomainPriceRange)range);

        await unitOfWork.SaveChangesAsync(ct);

        ApplyShopMenuSnapshotEvent? outbound = null;
        if (shop.ShopId != Guid.Empty && MenuDraftMapper.ToSnapshot(shop.Menu) is { } snapshot)
            outbound = new ApplyShopMenuSnapshotEvent(shop.ShopId, snapshot, command.ApplySuggestedPriceRange, command.UserId);

        return (Response<ModerationShopDto>.Success(mapper.Map<ModerationShopDto>(shop)), outbound);
    }
}
