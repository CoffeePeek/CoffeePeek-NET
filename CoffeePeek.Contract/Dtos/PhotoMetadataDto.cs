namespace CoffeePeek.Contract.Dtos;

public class PhotoMetadataDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public string StorageKey { get; init; } = null!;
    public string? FullUrl { get; init; }
    public long SizeBytes { get; init; }
    public Guid OwnerId { get; init; }
    public DateTime UploadedAt { get; init; }
    public int SortIndex { get; init; }
}
