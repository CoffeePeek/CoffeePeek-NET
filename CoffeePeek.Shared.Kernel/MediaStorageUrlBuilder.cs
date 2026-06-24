namespace CoffeePeek.Shared.Kernel;

public static class MediaStorageUrlBuilder
{
    public static string? BuildPublicUrl(string? publicEndpoint, string bucketName, string? storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)
            || string.IsNullOrWhiteSpace(publicEndpoint)
            || string.IsNullOrWhiteSpace(bucketName))
        {
            return null;
        }

        return $"{publicEndpoint.TrimEnd('/')}/{bucketName.Trim('/')}/{storageKey.TrimStart('/')}";
    }
}
