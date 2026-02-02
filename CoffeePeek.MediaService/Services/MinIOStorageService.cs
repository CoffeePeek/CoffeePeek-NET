using CoffeePeek.MediaService.Configuration;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;
using Minio.Exceptions;

namespace CoffeePeek.MediaService.Services;

public class MinIOStorageService(IMinioClient minioClient, IOptions<MinIOOptions> options) : IStorageService
{
    private const string IsPermanentTag = "is_permanent";
    private const int PresignedUrlExpirySeconds = 600;
    

    public async Task<(string UploadUrl, string StorageKey)> GetPresignedUploadUrlAsync(string fileName,
        string contentType, BucketType bucketType)
    {
        var storageKey = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        var args = new PresignedPutObjectArgs()
            .WithBucket(GetBucketName(bucketType))
            .WithObject(storageKey)
            .WithHeaders(new Dictionary<string, string> {
                { "Content-Type", contentType },
                { "x-amz-tagging", $"{IsPermanentTag}={false}" }
            })
            .WithExpiry(PresignedUrlExpirySeconds);

        var url = await minioClient.PresignedPutObjectAsync(args);

        return (url, storageKey);
    }

    public async Task MarkAsPermanentAsync(string storageKey, BucketType bucketType)
    {
        var tags = new Dictionary<string, string>
        {
            [IsPermanentTag] = "true"
        };

        var args = new SetObjectTagsArgs()
            .WithBucket(GetBucketName(bucketType))
            .WithObject(storageKey)
            .WithTagging(Tagging.GetObjectTags(tags));

        await minioClient.SetObjectTagsAsync(args);
    }

    public async Task<bool> ExistsAsync(string storageKey, BucketType bucketType)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(GetBucketName(bucketType))
                .WithObject(storageKey);

            await minioClient.StatObjectAsync(args);
            return true;
        }
        catch (MinioException)
        {
            return false;
        }
    }

    public async Task DeleteAsync(string storageKey, BucketType bucketType)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(GetBucketName(bucketType))
            .WithObject(storageKey);

        await minioClient.RemoveObjectAsync(args);
    }

    private string GetBucketName(BucketType type)
    {
        return type switch
        {
            BucketType.User => options.Value.UserBucketName,
            BucketType.Shop => options.Value.ShopBucketName,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported bucket type")
        };
    }
}