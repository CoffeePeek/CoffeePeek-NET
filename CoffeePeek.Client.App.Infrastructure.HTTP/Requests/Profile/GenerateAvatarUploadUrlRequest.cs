namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Profile;

public sealed class GenerateAvatarUploadUrlRequest : BaseRequest
{
    public required int SizeBytes { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }
}
