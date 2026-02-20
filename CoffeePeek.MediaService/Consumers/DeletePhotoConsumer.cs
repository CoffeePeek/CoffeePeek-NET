using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Events;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.MediaService.Consumers;

public class DeletePhotoConsumer(
    IPhotoRepository photoRepository,
    IUnitOfWork unitOfWork,
    IStorageService storageService) : IConsumer<DeletePhotoFromStorageEvent>
{
    public async Task Consume(ConsumeContext<DeletePhotoFromStorageEvent> context)
    {
        var command = context.Message;
        var ct = context.CancellationToken;

        var photo = await photoRepository.GetByIdAsync(command.PhotoId, ct);
        
        if (photo == null || photo.Status == PhotoStatus.Deleted) return;

        await storageService.Delete(photo.StorageKey, (BucketType)(int)photo.BucketType, ct);

        photo.Status = PhotoStatus.Deleted;
        photo.DeletedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(ct);
    }
}