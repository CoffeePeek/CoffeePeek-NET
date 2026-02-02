using CoffeePeek.MediaService.Configuration;

namespace CoffeePeek.MediaService.Services;

public interface IStorageService
{
    Task<(string UploadUrl, string StorageKey)> GetPresignedUploadUrlAsync(string fileName, string contentType, BucketType bucketType);
    
    Task MarkAsPermanentAsync(string storageKey, BucketType bucketType);
    Task<bool> ExistsAsync(string uploadedPhotoStorageKey, BucketType bucketType);
    Task DeleteAsync(string oldStorageKey, BucketType bucketType);
}