using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Services;

namespace CoffeePeek.MediaService.Handlers;

public record DeletePhotoFromStorage(Guid PhotoId);

public static class DeletePhotoHandler
{
    public static async Task Handle(
        DeletePhotoFromStorage command,
        MediaDbContext dbContext,
        IStorageService storageService,
        ILogger logger,
        CancellationToken ct)
    {
        var photo = await dbContext.Photos.FindAsync([command.PhotoId], ct);
        if (photo == null || photo.Status == PhotoStatus.Deleted) return;

        try
        {
            await storageService.Delete(photo.StorageKey, (Configuration.BucketType)(int)photo.BucketType, ct);
            
            photo.Status = PhotoStatus.Deleted;
            photo.DeletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Retryable failure deleting photo {Id}", command.PhotoId);
            throw; 
        }
    }
}