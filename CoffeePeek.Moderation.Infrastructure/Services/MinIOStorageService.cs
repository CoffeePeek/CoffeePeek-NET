using Amazon.S3;
using Amazon.S3.Model;
using CoffeePeek.Moderation.Application.Services;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Moderation.Infrastructure.Services;

public class MinIOStorageService(IAmazonS3 s3Client, IOptions<MinIOOptions> options) : IStorageService
{
    private readonly string _bucketName = options.Value.BucketName;
    
    public async Task<string> UploadFileAsync(Stream stream, string contentType, CancellationToken ct)
    {
        var storageKey = Guid.NewGuid().ToString();
        
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = storageKey,
            InputStream = stream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await s3Client.PutObjectAsync(request, ct);
        return storageKey;
    }

    public string GetFileUrl(string fileKey) => $"/{_bucketName}/{fileKey}";
    
    public async Task DeleteFileAsync(string fileKey, CancellationToken ct)
    {
        await s3Client.DeleteObjectAsync(_bucketName, fileKey, ct);
    }
}