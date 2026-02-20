using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;

namespace CoffeePeek.MediaService.Factories;

public static class PhotoMetadataFactory
{
    public static PhotoMetadata Create(
        string fileName,
        string contentType,
        string storageKey,
        long sizeBytes,
        BucketType bucketType,
        OwnerType ownerType,
        Guid ownerId) => new()
    {
        Id = Guid.NewGuid(),
        FileName = fileName,
        ContentType = contentType,
        StorageKey = storageKey,
        SizeBytes = sizeBytes,
        BucketType = bucketType,
        OwnerType = ownerType,
        OwnerId = ownerId,
        Status = PhotoStatus.Pending,
        UploadedAt = DateTime.UtcNow
    };
}