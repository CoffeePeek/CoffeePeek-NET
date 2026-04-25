namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;

public sealed class ShopPhotoUploadRequest
{
    public required int SizeBytes { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }
}
