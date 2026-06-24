using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Infrastructure.Options;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Options;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Account.Infrastructure.Media;

public class MediaUrlProvider(
    IOptions<MinIOOptions> minioOptions,
    IOptions<MediaPublicUrlOptions> mediaPublicOptions) : IMediaUrlProvider
{
    public string? BuildAvatarUrl(string storageKey)
    {
        var bucketName = !string.IsNullOrWhiteSpace(minioOptions.Value.BucketName)
            ? minioOptions.Value.BucketName
            : mediaPublicOptions.Value.AvatarBucketName;

        var publicEndpoint = !string.IsNullOrWhiteSpace(mediaPublicOptions.Value.PublicEndpoint)
            ? mediaPublicOptions.Value.PublicEndpoint
            : minioOptions.Value.Endpoint;

        return MediaStorageUrlBuilder.BuildPublicUrl(publicEndpoint, bucketName, storageKey);
    }
}
