using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Data;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.MediaService.Services;

/// <summary>
/// Service responsible for cleaning up old photos from MinIO storage
/// when they are replaced by new ones.
/// </summary>
public class PhotoCleanupService(
    IStorageService storageService,
    MediaDbContext dbContext,
    ILogger<PhotoCleanupService> logger)
{
    /// <summary>
    /// Delay before deleting old photos to ensure new ones are properly saved.
    /// </summary>
    private static readonly TimeSpan CleanupDelay = TimeSpan.FromHours(1);

    /// <summary>
    /// Handles the PhotoReplacedEvent by scheduling old photo deletion.
    /// </summary>
    public async Task HandlePhotoReplacedAsync(PhotoReplacedEvent eventData, CancellationToken ct)
    {
        logger.LogInformation(
            "Photo {OldPhotoId} replaced by {NewPhotoId} for {OwnerType} {OwnerId}. Scheduling cleanup.",
            eventData.OldPhotoId, eventData.NewPhotoId, eventData.OwnerType, eventData.OwnerId);

        // Mark old photo as scheduled for deletion in database
        var oldPhoto = await dbContext.Photos.FindAsync([eventData.OldPhotoId], ct);
        if (oldPhoto != null)
        {
            oldPhoto.Status = PhotoStatus.PendingDeletion;
            oldPhoto.ScheduledDeletionAt = DateTime.UtcNow.Add(CleanupDelay);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Processes pending photo deletions that are due.
    /// Should be called by a background job.
    /// </summary>
    public async Task ProcessPendingDeletionsAsync(CancellationToken ct)
    {
        var photosToDelete = await dbContext.Photos
            .Where(p => p.Status == PhotoStatus.PendingDeletion &&
                        p.ScheduledDeletionAt <= DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var photo in photosToDelete)
        {
            try
            {
                // Delete from MinIO
                await storageService.Delete(photo.StorageKey, (Configuration.BucketType)(int)photo.BucketType, ct);

                // Mark as deleted in database
                photo.Status = PhotoStatus.Deleted;
                photo.DeletedAt = DateTime.UtcNow;

                logger.LogInformation("Deleted old photo {PhotoId} with storage key {StorageKey} from MinIO",
                    photo.Id, photo.StorageKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete photo {PhotoId} from MinIO", photo.Id);
            }
        }

        if (photosToDelete.Count > 0)
        {
            await dbContext.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Immediately deletes a photo from MinIO and marks it as deleted.
    /// Use when immediate deletion is required.
    /// </summary>
    public async Task DeletePhotoImmediatelyAsync(Guid photoId, CancellationToken ct)
    {
        var photo = await dbContext.Photos.FindAsync([photoId], ct);
        if (photo == null)
        {
            logger.LogWarning("Photo {PhotoId} not found for immediate deletion", photoId);
            return;
        }

        try
        {
            await storageService.Delete(photo.StorageKey, (Configuration.BucketType)(int)photo.BucketType);
            photo.Status = PhotoStatus.Deleted;
            photo.DeletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation("Immediately deleted photo {PhotoId}", photoId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to immediately delete photo {PhotoId}", photoId);
            throw;
        }
    }
}
