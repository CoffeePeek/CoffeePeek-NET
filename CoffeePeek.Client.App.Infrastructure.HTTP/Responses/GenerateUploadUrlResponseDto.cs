namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GenerateUploadUrlResponseDto
{
    public Guid PhotoId { get; init; }

    public string UploadUrl { get; init; } = string.Empty;

    public string StorageKey { get; init; } = string.Empty;
}
