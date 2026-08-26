using CoffeePeek.Contract.Dtos.Menu;
using CoffeePeek.Contract.Events.Menu;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Options;
using CoffeePeek.Shops.Application.Abstractions;
using CoffeePeek.Shops.Application.Features.Menu.ParseMenuPhotos;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shops.Infrastructure.Consumers;

public class ParseMenuRequestedEventHandler(
    IApplyShopMenuService applyMenu,
    IQueryCoffeeShopRepository shopRepository,
    IUnitOfWork unitOfWork,
    IOptions<MediaPublicUrlOptions> mediaOptions,
    IMenuVisionParser parser,
    IQueryCoffeeDrinkRepository drinks,
    IMenuPhotoDownloader downloader,
    IOptions<MenuPriceRangeOptions> priceOptions,
    ILogger<ParseMenuRequestedEventHandler> logger)
{
    public async Task<MenuParsedEvent> Handle(ParseMenuRequestedEvent message, CancellationToken ct)
    {
        var media = mediaOptions.Value;
        var inputs = message.Photos
            .Select(p => new ParseMenuPhotoInput(
                p.StorageKey,
                p.ContentType,
                MediaStorageUrlBuilder.BuildPublicUrl(media.PublicEndpoint, media.ShopBucketName, p.StorageKey)))
            .ToArray();

        var parsed = await ParseMenuPhotosHandler.Handle(
            new ParseMenuPhotosCommand(inputs),
            parser,
            drinks,
            downloader,
            priceOptions,
            ct);

        var data = parsed.Data ?? new ParseMenuPhotosResponse(false, "Parse failed.", null, [], []);
        var capturedAt = DateTime.UtcNow;
        var shopId = message.PublishedShopId
                     ?? await shopRepository.GetIdByModerationId(message.SourceId, ct);

        if (shopId.HasValue)
        {
            if (data.Success)
            {
                await applyMenu.ApplyParseResultAsync(
                    shopId.Value,
                    data.Items,
                    data.Unmatched,
                    data.SuggestedPriceRange,
                    message.Photos.Select(p => new ShopMenuPhotoSnapshot(
                        p.FileName, p.ContentType, p.StorageKey, p.SizeBytes, p.MediaPhotoId)).ToArray(),
                    capturedAt,
                    message.RequestedByUserId,
                    ct);
            }
            else
            {
                await applyMenu.MarkParseFailedAsync(shopId.Value, data.Error ?? "Parse failed.", ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "Menu parse for {Kind} {SourceId} success={Success} shop={ShopId}",
            message.SourceKind,
            message.SourceId,
            data.Success,
            shopId);

        return new MenuParsedEvent(
            message.SourceKind,
            message.SourceId,
            shopId,
            data.Success,
            data.Error,
            data.SuggestedPriceRange,
            data.Items,
            data.Unmatched,
            capturedAt);
    }
}

public class ApplyShopMenuSnapshotEventHandler(
    IApplyShopMenuService applyMenu,
    IUnitOfWork unitOfWork)
{
    public async Task Handle(ApplyShopMenuSnapshotEvent message, CancellationToken ct)
    {
        await applyMenu.ApplySnapshotAsync(
            message.ShopId,
            message.Snapshot,
            message.ApplySuggestedPriceRange,
            message.UserId,
            ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
