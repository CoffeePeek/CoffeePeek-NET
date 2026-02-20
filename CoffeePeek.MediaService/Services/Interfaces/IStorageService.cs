using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;

namespace CoffeePeek.MediaService.Services;

public interface IStorageService
{
    Task<PresignedPhotoMetaData> GetPresignedUploadUrl(string fileName, string contentType, BucketType bucketType, CancellationToken ct = default);
    
    Task MarkAsPermanent(string storageKey, BucketType bucketType, CancellationToken ct = default);
    Task<bool> Exists(string uploadedPhotoStorageKey, BucketType bucketType, CancellationToken ct = default);
    Task Delete(string oldStorageKey, BucketType bucketType, CancellationToken ct = default);
}