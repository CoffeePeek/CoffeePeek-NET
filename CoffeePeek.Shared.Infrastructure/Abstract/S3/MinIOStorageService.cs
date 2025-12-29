using CoffeePeek.Moderation.Infrastructure;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;
using Minio.Exceptions;

namespace CoffeePeek.Shared.Infrastructure.Abstract.S3;

public class MinIOStorageService(IMinioClient minioClient, IOptions<MinIOOptions> options) : IStorageService
{
    private const string IsPermanentTag = "is_permanent";
    private const int PresignedUrlExpirySeconds = 600;
    
    private readonly string _bucketName = options.Value.BucketName;

    public async Task<(string UploadUrl, string StorageKey)> GetPresignedUploadUrlAsync(string fileName,
        string contentType)
    {
        var storageKey = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        var args = new PresignedPutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(storageKey)
            .WithHeaders(new Dictionary<string, string> {
                { "Content-Type", contentType },
                { "x-amz-tagging", $"{IsPermanentTag}={false}" }
            })
            .WithExpiry(PresignedUrlExpirySeconds);

        var url = await minioClient.PresignedPutObjectAsync(args);

        return (url, storageKey);
    }

    public async Task MarkAsPermanentAsync(string storageKey)
    {
        var tags = new Dictionary<string, string>
        {
            [IsPermanentTag] = "true"
        };

        var args = new SetObjectTagsArgs()
            .WithBucket(_bucketName)
            .WithObject(storageKey)
            .WithTagging(Tagging.GetObjectTags(tags));

        await minioClient.SetObjectTagsAsync(args);
    }

    public async Task<bool> ExistsAsync(string storageKey)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(storageKey);

            await minioClient.StatObjectAsync(args);
            return true;
        }
        catch (MinioException)
        {
            return false;
        }
    }

    public async Task DeleteAsync(string storageKey)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(storageKey);

        await minioClient.RemoveObjectAsync(args);
    }
}