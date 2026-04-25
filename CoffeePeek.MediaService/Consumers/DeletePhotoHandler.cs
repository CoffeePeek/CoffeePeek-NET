using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Events;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.MediaService.Consumers;

public class DeletePhotoHandler(
    IPhotoRepository photoRepository,
    IUnitOfWork unitOfWork,
    IStorageService storageService)
{
    public async Task Handle(DeletePhotoFromStorageEvent message)
    {
        var photo = await photoRepository.GetByIdAsync(message.PhotoId);
        
        if (photo == null || photo.Status == PhotoStatus.Deleted) return;

        await storageService.Delete(photo.StorageKey, (BucketType)(int)photo.BucketType);

        photo.Status = PhotoStatus.Deleted;
        photo.DeletedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();
    }
}