using CoffeePeek.MediaService.Configuration;

namespace CoffeePeek.MediaService.Services;

public interface IStorageService
{
    Task<(string UploadUrl, string StorageKey)> GetPresignedUploadUrl(string fileName, string contentType, BucketType bucketType, CancellationToken ct = default);
    
    Task MarkAsPermanent(string storageKey, BucketType bucketType, CancellationToken ct = default);
    Task<bool> Exists(string uploadedPhotoStorageKey, BucketType bucketType, CancellationToken ct = default);
    Task Delete(string oldStorageKey, BucketType bucketType, CancellationToken ct = default);
}