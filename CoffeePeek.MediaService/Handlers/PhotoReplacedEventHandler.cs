using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;

namespace CoffeePeek.MediaService.Handlers;

public class PhotoReplacedEventHandler(
    PhotoCleanupService cleanupService,
    ILogger<PhotoReplacedEventHandler> logger) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Media.PhotoReplaced)]
    public async Task HandleAsync(PhotoReplacedEvent eventData, CancellationToken ct)
    {
        logger.LogInformation(
            "Received PhotoReplacedEvent for old photo {OldPhotoId}, new photo {NewPhotoId}",
            eventData.OldPhotoId, eventData.NewPhotoId);

        await cleanupService.HandlePhotoReplacedAsync(eventData, ct);
    }
}
