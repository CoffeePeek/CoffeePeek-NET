using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Data;
using JasperFx.Core;
using Wolverine;

namespace CoffeePeek.MediaService.Handlers;

public class PhotoReplacedEventHandler
{
    public async Task Handle(
        PhotoReplacedEvent @event,
        MediaDbContext dbContext,
        IMessageContext bus,
        CancellationToken ct)
    {
        var oldPhoto = await dbContext.Photos.FindAsync([@event.OldPhotoId], ct);
        if (oldPhoto != null)
        {
            oldPhoto.Status = PhotoStatus.PendingDeletion;
            await dbContext.SaveChangesAsync(ct);

            await bus.ScheduleAsync(new DeletePhotoFromStorage(@event.OldPhotoId), 1.Hours());
        }
    }
}
