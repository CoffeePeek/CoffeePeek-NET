namespace CoffeePeek.Moderation.Domain.Entities;

public class PhotoMetadata(string fileName, string contentType, string storageKey, long sizeBytes, Guid ownerId, Guid moderationShopId)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; private set; } = fileName;
    public string ContentType { get; private set; } = contentType;
    public string StorageKey { get; private set; } = storageKey;
    public long SizeBytes { get; private set; } = sizeBytes;
    public Guid OwnerId { get; private set; } = ownerId;
    public Guid ModerationShopId { get; private set; } = moderationShopId;
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;
}