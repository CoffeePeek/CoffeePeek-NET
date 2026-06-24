using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Shared.Kernel.Options;

/// <summary>
/// Public URLs for objects stored in MinIO (returned to clients in API responses).
/// </summary>
public class MediaPublicUrlOptions
{
    /// <summary>
    /// Public base URL for object storage (e.g. https://media.coffeepeek.by).
    /// Path-style URLs: {PublicEndpoint}/{bucket}/{storageKey}
    /// </summary>
    [Url]
    public string PublicEndpoint { get; set; } = string.Empty;

    public string ShopBucketName { get; set; } = "coffeepeek.shops";

    public string AvatarBucketName { get; set; } = "coffeepeek.avatars";
}
