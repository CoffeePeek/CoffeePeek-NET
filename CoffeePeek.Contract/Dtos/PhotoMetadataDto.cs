namespace CoffeePeek.Contract.Dtos;

public class PhotoMetadataDto
{
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public string StorageKey { get; init; } = null!;
    public string? FullUrl { get; init; }
    public long SizeBytes { get; init; }
    public Guid OwnerId { get; init; }
    public DateTime UploadedAt { get; init; }
}