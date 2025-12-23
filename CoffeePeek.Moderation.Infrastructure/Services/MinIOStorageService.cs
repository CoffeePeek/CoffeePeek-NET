using Coffeepeek.Moderation.Application.Abstractions;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;

namespace CoffeePeek.Moderation.Infrastructure.Services;

public class MinIOStorageService(IMinioClient minioClient, IOptions<MinIOOptions> options) : IStorageService
{
    private readonly string _bucketName = options.Value.BucketName;

    public async Task<(string UploadUrl, string StorageKey)> GetPresignedUploadUrlAsync(string fileName,
        string contentType)
    {
        var storageKey = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        const string tagging = "is_permanent=false";

        var args = new PresignedPutObjectArgs()
            .WithBucket("coffee.shops")
            .WithObject(storageKey)
            .WithHeaders(new Dictionary<string, string> {
                { "Content-Type", contentType },
                { "x-amz-tagging", tagging }
            })
            .WithExpiry(600);

        var url = await minioClient.PresignedPutObjectAsync(args);

        return (url, storageKey);
    }

    public async Task MarkAsPermanentAsync(string storageKey)
    {
        var tags = new Dictionary<string, string> { { "is_permanent", "true" } };

        await minioClient.SetObjectTagsAsync(new SetObjectTagsArgs()
            .WithBucket(_bucketName)
            .WithObject(storageKey)
            .WithTagging(Tagging.GetObjectTags(tags)));
    }
}