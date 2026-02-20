using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Events;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.MediaService.Consumers;

public class PhotoReplacedConsumer(
    IPhotoRepository photoRepository,
    IUnitOfWork unitOfWork,
    IMessageScheduler scheduler) : IConsumer<PhotoReplacedEvent>
{
    public async Task Consume(ConsumeContext<PhotoReplacedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;

        var oldPhoto = await photoRepository.GetByIdAsync(@event.OldPhotoId, ct);
        
        if (oldPhoto == null) return;

        oldPhoto.Status = PhotoStatus.PendingDeletion;

        await scheduler.SchedulePublish(
            DateTime.UtcNow.AddHours(1),
            new DeletePhotoFromStorageEvent(@event.OldPhotoId, oldPhoto.StorageKey),
            ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}