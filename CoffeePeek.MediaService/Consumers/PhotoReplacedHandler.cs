using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Events;
using CoffeePeek.MediaService.Repositories;
using Wolverine;
using Wolverine.EntityFrameworkCore.Internals;

namespace CoffeePeek.MediaService.Consumers;

public class PhotoReplacedHandler(IPhotoRepository photoRepository)
{
    public async Task<object?> Handle(PhotoReplacedEvent message)
    {
        var oldPhoto = await photoRepository.GetByIdAsync(message.OldPhotoId);

        if (oldPhoto == null) return null;

        oldPhoto.Status = PhotoStatus.PendingDeletion;

        return new OutgoingMessage(
            new Envelope(new DeletePhotoFromStorageEvent(message.OldPhotoId, oldPhoto.StorageKey))
            {
                DeliverWithin = TimeSpan.FromHours(1)
            });
    }
}